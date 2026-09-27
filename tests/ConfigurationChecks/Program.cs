using System;
using System.Linq;
using M365Debloater;

internal static class Program
{
    private static int _checks;

    private static void Main()
    {
        Check(OfficeConfiguration.DetectProduct(null) == null, "Missing Office must not select a default product.");
        Check(OfficeConfiguration.DetectProduct("UnknownRetail") == null, "Unknown editions must be rejected.");
        Check(OfficeConfiguration.DetectProduct("O365BusinessRetailExtra") == null, "Product matching must be exact.");
        Check(OfficeConfiguration.DetectProduct(" o365businessretail ,VisioProRetail") == "O365BusinessRetail", "Match one known suite with a companion product.");
        Check(OfficeConfiguration.DetectProduct("O365BusinessRetail,O365ProPlusRetail") == null, "Ambiguous suites must be rejected.");
        Check(OfficeConfiguration.DetectChannel("O365BusinessRetail", "unknown", "") == null, "Unknown channels must not become Current.");
        Check(OfficeConfiguration.DetectChannel("ProPlus2024Volume", "", "") == "PerpetualVL2024", "LTSC must use its own channel.");
        Check(OfficeConfiguration.DetectChannel("O365BusinessRetail", "", "https://officecdn.microsoft.com/pr/55336b82-a18d-4dd6-b5f6-9e5095c314a6") == "MonthlyEnterprise", "Use UpdateChannel when CDNBaseUrl is absent.");
        Check(OfficeConfiguration.DetectChannel("O365BusinessRetail", "https://officecdn.microsoft.com/pr/b8f9b850-328d-4355-9145-c59439a0c4cf", "") == "SemiAnnualPreview", "Semi-Annual Preview must not switch to Beta.");
        Check(OfficeConfiguration.DetectChannel("O365BusinessRetail", "https://officecdn.microsoft.com/pr/64256afe-f5d9-4f86-8936-8840a6a4f5be", "") == "CurrentPreview", "Recognize Current Preview.");
        Check(OfficeConfiguration.DetectChannel("O365BusinessRetail", "https://officecdn.microsoft.com/pr/5440fd1f-7ecb-4221-8110-145efaa6372f", "") == "BetaChannel", "Recognize the Beta channel identifier.");

        var oneDrive = OfficeConfiguration.Apps.Single(app => app.Name == "OneDrive");
        var document = OfficeConfiguration.Create("O365BusinessRetail", "32", "MonthlyEnterprise",
            oneDrive.ExclusionIds.Concat(new[] { "Access" }), new[] { "Word", "access" });
        var add = document.Root.Element("Add");
        var product = add.Element("Product");
        var excluded = product.Elements("ExcludeApp").Select(item => (string)item.Attribute("ID")).ToArray();
        Check(excluded.SequenceEqual(new[] { "Access", "Groove", "OneDrive", "Word" }), "Retain existing exclusions, deduplicate and cover both OneDrive clients.");
        Check((string)add.Attribute("OfficeClientEdition") == "32", "Preserve detected architecture.");
        Check((string)add.Attribute("Channel") == "MonthlyEnterprise", "Preserve detected channel.");
        Check((string)add.Attribute("Version") == "MatchInstalled", "Request the installed version.");
        Check((string)product.Attribute("ID") == "O365BusinessRetail", "Preserve detected edition.");
        Check((string)product.Element("Language").Attribute("ID") == "MatchInstalled", "Request installed languages.");
        Check((string)document.Root.Element("Property").Attribute("Value") == "FALSE", "Do not force Office apps to close.");
        Check(!document.Descendants("Remove").Any(), "Do not uninstall the entire suite.");
        Reject(() => OfficeConfiguration.Create(null, "64", "Current", new[] { "Access" }, new string[0]));
        Reject(() => OfficeConfiguration.Create("O365BusinessRetail", "ARM", "Current", new[] { "Access" }, new string[0]));
        Reject(() => OfficeConfiguration.Create("O365BusinessRetail", "64", null, new[] { "Access" }, new string[0]));
        Reject(() => OfficeConfiguration.Create("O365BusinessRetail", "64", "Current", new[] { "Access" }, new[] { "UnknownApp" }));
        Console.WriteLine("Passed " + _checks + " configuration checks. No Windows operations were executed.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        _checks++;
    }

    private static void Reject(Action action)
    {
        try { action(); }
        catch (InvalidOperationException) { _checks++; return; }
        throw new Exception("Invalid configuration was accepted.");
    }
}
