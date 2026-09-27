using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace M365Debloater
{
    internal sealed class OfficeApp
    {
        public string Name { get; private set; }
        public string[] ExclusionIds { get; private set; }

        public OfficeApp(string name, params string[] exclusionIds)
        {
            Name = name;
            ExclusionIds = exclusionIds;
        }

        public override string ToString() { return Name; }
    }

    // ODT identifiers are independent of the labels used by the interface.
    internal static class OfficeConfiguration
    {
        public static readonly OfficeApp[] Apps =
        {
            new OfficeApp("Access", "Access"),
            new OfficeApp("OneDrive", "OneDrive", "Groove"),
            new OfficeApp("OneNote", "OneNote"),
            new OfficeApp("Outlook (classic)", "Outlook"),
            new OfficeApp("Publisher", "Publisher"),
            new OfficeApp("Skype for Business", "Lync"),
            new OfficeApp("Microsoft Teams", "Teams")
        };

        private static readonly string[] Products =
        {
            "O365ProPlusRetail", "O365BusinessRetail", "ProPlus2019Retail",
            "ProPlus2021Retail", "ProPlus2024Volume"
        };

        private static readonly string[] ExclusionIds =
        {
            "Access", "Excel", "Groove", "Lync", "OneDrive", "OneNote", "Outlook",
            "OutlookForWindows", "PowerPoint", "Publisher", "Teams", "Word"
        };

        public static string DetectProduct(string releaseIds)
        {
            var installed = SplitIds(releaseIds);
            var matches = Products.Where(p => installed.Contains(p, StringComparer.OrdinalIgnoreCase)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        public static string DetectChannel(string product, string cdnBaseUrl, string updateChannel)
        {
            if (product == "ProPlus2024Volume") return "PerpetualVL2024";
            string source = string.IsNullOrWhiteSpace(cdnBaseUrl) ? updateChannel ?? "" : cdnBaseUrl;
            var channels = new Dictionary<string, string>
            {
                { "492350f6-3a01-4f97-b9c0-c7c6ddf67d60", "Current" },
                { "55336b82-a18d-4dd6-b5f6-9e5095c314a6", "MonthlyEnterprise" },
                { "7ffbc6bf-bc32-4f92-8982-f9dd17fd3114", "SemiAnnual" },
                { "b8f9b850-328d-4355-9145-c59439a0c4cf", "SemiAnnualPreview" },
                { "64256afe-f5d9-4f86-8936-8840a6a4f5be", "CurrentPreview" },
                { "5440fd1f-7ecb-4221-8110-145efaa6372f", "BetaChannel" }
            };
            return channels.Where(c => source.IndexOf(c.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(c => c.Value).FirstOrDefault();
        }

        public static string[] SplitIds(string value)
        {
            return (value ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim()).Where(id => id.Length > 0).ToArray();
        }

        public static XDocument Create(string product, string architecture, string channel,
            IEnumerable<string> selected, IEnumerable<string> existing)
        {
            if (!Products.Contains(product) || (architecture != "32" && architecture != "64")
                || string.IsNullOrWhiteSpace(channel))
                throw new InvalidOperationException("A recognized Office installation is required.");

            var exclusions = selected.Concat(existing).Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(id => ExclusionIds.FirstOrDefault(known => known.Equals(id, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException("Unrecognized existing exclusion: " + id))
                .OrderBy(id => id, StringComparer.Ordinal).ToArray();

            return new XDocument(new XElement("Configuration",
                new XElement("Add", new XAttribute("OfficeClientEdition", architecture),
                    new XAttribute("Channel", channel), new XAttribute("Version", "MatchInstalled"),
                    new XElement("Product", new XAttribute("ID", product),
                        new XElement("Language", new XAttribute("ID", "MatchInstalled")),
                        exclusions.Select(id => new XElement("ExcludeApp", new XAttribute("ID", id))))),
                new XElement("Property", new XAttribute("Name", "FORCEAPPSHUTDOWN"), new XAttribute("Value", "FALSE")),
                new XElement("Display", new XAttribute("Level", "None"), new XAttribute("AcceptEULA", "TRUE"))));
        }
    }
}
