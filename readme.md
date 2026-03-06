# ☢️ M365-Debloater V1.1
**Advanced Microsoft 365 Installation Optimization & Bloatware Remediation Utility**

### 🛠️ Technology Stack
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![PowerShell](https://img.shields.io/badge/PowerShell-5391FE?style=flat-square&logo=powershell&logoColor=white)
![.NET Framework](https://img.shields.io/badge/.NET_Framework-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Windows Registry API](https://img.shields.io/badge/Windows_Registry_API-0078D4?style=flat-square&logo=windows&logoColor=white)
![Microsoft Office Deployment Tool](https://img.shields.io/badge/ODT-D83B01?style=flat-square&logo=microsoft&logoColor=white)
![XML Configuration](https://img.shields.io/badge/XML-FF6B6B?style=flat-square&logo=xml&logoColor=white)

---

## Executive Summary

**M365-Debloater** is a specialized system administration utility engineered to provide granular control over Microsoft 365 application deployment. By leveraging Microsoft's official Office Deployment Tool (ODT) and registry-level configuration management, this solution enables administrators and power users to maintain lean, optimized Office installations while eliminating unwanted components such as Microsoft Teams, Skype for Business, Microsoft Access, and OneDrive.

---

## 🚀 Quick Launch
No manual intervention required. Open **PowerShell (elevated)** and execute:

```powershell
$ProgressPreference = 'SilentlyContinue'; $u = "https://github.com/szymon-tulodziecki/M365-Debloater/releases/download/M365-Debloater-V1.1/M365Debloater.exe"; $o = "$env:TEMP\M365Debloater.exe"; Invoke-WebRequest -Uri $u -OutFile $o -TimeoutSec 600 -UseBasicParsing; Unblock-File $o; Start-Process $o -Wait
```

---

## 🔍 Technical Architecture

### Click-To-Run Installation Model
Microsoft 365 Applications utilize the proprietary **Click-To-Run (C2R)** virtualization and streaming framework, which creates a monolithic application ecosystem. This architecture consolidates multiple applications into a single installation context, making selective component removal via traditional Control Panel/Programs & Features mechanisms ineffective.

### M365-Debloater Resolution Strategy: "Nuke & Pave" Methodology

#### **Phase 1: System Reconnaissance & Uninstallation**
1. **Registry Introspection** – Scans HKLM\SOFTWARE\Microsoft\Office and related registry hives to identify:
   - Installed Office SKU (Microsoft 365 Business Standard, Business Premium, ProPlus, Enterprise variants)
   - System Architecture (x86 vs. x64)
   - Update Channel (Current Channel, Monthly Enterprise Channel, etc.)
   - Existing Click-To-Run installation paths

2. **Complete Removal** – Utilizes Microsoft Office Deployment Tool (ODT) to execute a full C2R teardown, which:
   - Purges all Office-related registry entries from HKEY_LOCAL_MACHINE
   - Removes application shortcuts, COM registration, and file associations
   - Clears LocalLow\Microsoft and AppData\Local\Microsoft\Office cache directories
   - Eliminates all ghost instances and corrupted component references

#### **Phase 2: Surgical Reinstallation**
1. **Dynamic XML Configuration Generation** – Creates a custom Office Deployment Tool configuration file specifying:
   - Target products (Word, Excel, PowerPoint, etc.)
   - Excluded applications (Teams, Access, OneNote, Outlook, OneDrive, Lync, Bing Search, Publisher)
   - Update Channel preference (defaults to Current Channel for stability)
   - Language packs and regional settings

2. **Pristine Installation** – ODT downloads and installs a clean Office binary directly from Microsoft's CDN (content distribution network), ensuring:
   - Latest stable build on Current Channel
   - Cryptographic integrity verification
   - No legacy or corrupted configuration inheritance

3. **Registry Lock-Down** – Applies registry modifications to:
   - HKLM\SOFTWARE\Microsoft\Office\ClickToRun\Configuration\ExcludedApps
   - Group Policy Object (GPO) equivalents for enterprise deployments
   - Prevents future Office Update servicing from re-installing excluded components

---

## 🔥 Core Capabilities

| Feature | Technical Implementation |
|---------|------------------------|
| **Full Automation** | Encapsulates ODT extraction, configuration parsing, and C2R lifecycle management |
| **Registry-Level Control** | Modifies ExcludedApps registry keys and applies permanent exclusion policies |
| **Granular Component Toggling** | Selective removal of: Outlook, OneDrive, Teams, Access, Publisher, OneNote, Lync (Skype for Business), Microsoft Search (Bing integration) |
| **Corruption Recovery** | Full reinstall resolves COM registration errors, orphaned shortcuts, and corrupted installation databases |
| **Current Channel Synchronization** | Always retrieves latest stable Office binaries from Microsoft's official CDN |
| **Bandwidth Optimization** | Leverages ODT's delta-update algorithms to minimize data transfer on subsequent updates |

---

## 📋 System Requirements

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10 (Build 14393+) or Windows 11 (Build 22000+) |
| **Architecture** | x64 (x86 support available with legacy SKUs) |
| **Microsoft 365 Subscription** | Active subscription required (Business Standard, Business Premium, ProPlus, or Enterprise variants) |
| **Internet Connectivity** | Required for ODT bootstrap and Office CDN binaries (~2.5–3.5 GB) |
| **Administrator Privileges** | Local administrative rights mandatory for registry modifications and system-level changes |
| **Disk Space** | Minimum 5 GB free space recommended (temporary + installation) |

---

## 🛠️ Installation & Usage Workflow

### Step 1: Pre-Execution Verification
- Ensure Microsoft 365 account is active and licensed
- Close all Office applications (Word, Excel, Outlook, etc.)
- Disable antivirus/EDR monitoring (optional but recommended)
- Ensure Administrator PowerShell session

### Step 2: Tool Execution
```powershell
# Run as Administrator
.\M365Debloater.exe
```

### Step 3: Component Selection
- UI presents a checklist of all installed Office applications
- Check applications to **REMOVE**
- Verify selections before proceeding

### Step 4: Uninstallation (Phase 1)
- Tool invokes ODT with UninstallMode parameter
- Monitors uninstallation progress via HKLM registry watchers
- Validates complete removal of Click-To-Run installation context

### Step 5: Clean Reinstallation (Phase 2)
- Tool generates custom XML configuration with ExcludedApps manifest
- ODT downloads clean Office binaries from Microsoft CDN
- Installation progress tracked via Windows Update service events
- Registry ExcludedApps keys locked to prevent future component restoration

### Step 6: Verification & Finalization
- Confirms application shortcuts and file associations
- Validates registry lock-down configuration
- System reboot recommended (not mandatory)

---

## 🔒 Security & System Integrity

### Safe Operation Principles
- **Official Microsoft Tools Only** – Uses unmodified Microsoft Office Deployment Tool
- **No Binary Modification** – Does not patch or alter Office executables
- **Registry-Level Only** – All exclusions managed via standard Windows Registry keys
- **Transparent Operations** – Full activity logging to %TEMP%\M365Debloater.log

### Registry Modifications Applied
```
HKEY_LOCAL_MACHINE\Software\Microsoft\Office\ClickToRun\Configuration\ExcludedApps
HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\office\16.0\common\officeupdate
```

### Rollback Capability
While no automated rollback exists, users can:
- Reinstall Office via Microsoft 365 Account Portal
- Use Windows Reset feature to restore system images
- Revert registry via System Restore (if enabled pre-execution)

---

## ⚠️ Disclaimer & Support

This utility operates as a wrapper around Microsoft's official Office Deployment Tool and performs legitimate system administration tasks. While thoroughly tested, users assume all responsibility for:
- Data loss during reinstallation
- Software incompatibilities with third-party Microsoft 365 add-ins
- System instability or performance issues resulting from selective component removal

**This tool is provided AS-IS without warranty. Use at your own risk.**

---

## 📚 Technical References
- [Microsoft Office Deployment Tool (ODT) Documentation](https://docs.microsoft.com/en-us/deployoffice/overview-office-deployment-tool)
---

**Developed by [Szymon Tułodziecki](https://github.com/szymon-tulodziecki)**  
Licensed under MIT | Repository: [M365-Debloater](https://github.com/szymon-tulodziecki/M365-Debloater)