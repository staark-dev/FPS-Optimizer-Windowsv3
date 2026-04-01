# Windows FPS Optimizer Pro

> A safe, step-by-step Windows gaming performance optimizer with full backup & restore support.

![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue)
![Framework](https://img.shields.io/badge/.NET-8.0-purple)
![Language](https://img.shields.io/badge/language-C%23%20%2F%20WPF-green)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

---

## What it does

Windows FPS Optimizer Pro applies a curated set of performance tweaks through a guided 7-step wizard. Every change is reversible — a Windows Restore Point and registry backup are created before anything is modified.

**No overclocking. No BIOS changes. No driver replacements.**

---

## Screenshots

<img width="1048" height="715" alt="1" src="https://github.com/user-attachments/assets/80c83146-9517-4c08-bc28-6c8ee566c421" />
<img width="1049" height="716" alt="2" src="https://github.com/user-attachments/assets/a9a89854-13bb-409f-a440-4de89802d80e" />
<img width="1048" height="719" alt="3" src="https://github.com/user-attachments/assets/38754526-4199-43a0-8ae9-34f0f38ea630" />
<img width="1048" height="718" alt="4" src="https://github.com/user-attachments/assets/e55f5683-081a-48c9-820e-a1be7beaef8e" />
<img width="1053" height="717" alt="5" src="https://github.com/user-attachments/assets/ecad4575-d934-4d97-bdd0-7ad7940b7f4e" />
<img width="1053" height="721" alt="6" src="https://github.com/user-attachments/assets/b7a354c1-edda-4a64-86ed-2749c39d3edb" />
<img width="1052" height="718" alt="7 1" src="https://github.com/user-attachments/assets/e63416cf-30d9-4156-8c31-815fd06e607d" />
<img width="1058" height="722" alt="7" src="https://github.com/user-attachments/assets/b1ce7d33-ac2b-4827-995c-c27d590e2bc4" />
<img width="1049" height="716" alt="8 1" src="https://github.com/user-attachments/assets/0b1073cc-f109-4723-9280-5b0b1be4bce7" />
<img width="1052" height="715" alt="8" src="https://github.com/user-attachments/assets/01eb69ab-fd05-42ee-81e4-178d29a0ca6d" />

---

## Features

- **7-step wizard** — Welcome → Scan → Backup → Profile → Configure → Execute → Done
- **System scan** — detects CPU, GPU, RAM, storage type, OS build and power plan
- **Full backup** before changes:
  - Windows System Restore Point (native)
  - Registry export (`.REG` files on Desktop)
  - Services state snapshot (`.CSV` on Desktop)
- **Optimization profiles** — pick a preset or customize every module individually:
  - 🛡️ **Safe** — 4 modules, zero risk
  - ⚖️ **Balanced** — 8 modules, recommended for most users
  - 🔥 **Gaming Hardcore** — all 13 modules, maximum FPS
- **Live log** — colored real-time output during execution
- **Bilingual UI** — Romanian / English, switchable at any time
- **Single `.exe`** — self-contained, no installer, no dependencies

---

## Optimization Modules

| # | Module | Category | Risk |
|---|--------|----------|------|
| 1 | Ultimate Performance Power Plan | ⚡ Power & CPU | Safe |
| 2 | CPU Optimization (Core Parking off, Boost max) | ⚡ Power & CPU | Safe |
| 3 | RAM Optimization (lock kernel in RAM) | 💾 Memory & Storage | Safe |
| 4 | SSD/HDD Storage Optimization (TRIM, NTFS) | 💾 Memory & Storage | Safe |
| 5 | GPU & DirectX Optimization (Priority 8, GPU Scheduling) | 🎮 Graphics | Safe |
| 6 | Registry FPS Tweaks (SystemResponsiveness, Win32Priority) | 🎮 Graphics | Safe |
| 7 | Network — Minimum Latency (Nagle off, QoS 20% freed) | 🌐 Network | Safe |
| 8 | Visual Effects — Max Performance | 👁️ Interface | Cosmetic |
| 9 | Telemetry & Tracking Disabled | 🛡️ Privacy | Safe |
| 10 | Disable 23 Unnecessary Background Services | ⚙️ Services | Medium |
| 11 | Remove 35+ Preinstalled Bloatware Apps | 🗑️ Applications | Medium |
| 12 | Disable Scheduled Telemetry Tasks | 🗑️ Applications | Safe |
| 13 | Clean Temp Files, DNS Cache, WinSxS temp | 🧹 Cleanup | Safe |

---

## Requirements

| | |
|---|---|
| OS | Windows 10 / Windows 11 (x64) |
| Rights | **Administrator** (required) |
| Runtime | None — fully self-contained |
| .NET | Bundled (no install needed) |

---

## Build from Source

```bash
# Clone
git clone https://github.com/your-username/FPSOptimizerCS.git
cd FPSOptimizerCS

# Debug run
dotnet run

# Release — single self-contained EXE
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Output: `bin\Release\net8.0-windows\win-x64\publish\FPS Optimizer v1.0.3.exe`

> Make sure the application is **not running** before publishing.

---

## Restore / Undo

Everything can be reverted:

1. **Windows System Restore** — `Win + S` → *Create a restore point* → *System Restore* → pick the `FPSOptimizer_YYYYMMDD` point
2. **Registry** — double-click any `.REG` file saved on Desktop
3. **Services** — use the `.CSV` file on Desktop to re-enable services manually via `services.msc`
4. **Bloatware** — all removed apps can be reinstalled from Microsoft Store

---

## Project Structure

```
FPSOptimizerCS/
├── App.xaml / App.xaml.cs          # Application entry + global error handler
├── FPSOptimizer.csproj             # Project config (net8.0-windows, single-file)
├── app.manifest                    # Requires Administrator
│
├── Core/
│   ├── OptimizationOptions.cs      # Module definitions & user selections
│   ├── OptimizerEngine.cs          # Executes all optimizations
│   └── SystemScanner.cs            # Hardware detection (CPU/GPU/RAM/Disk/OS)
│
├── Localization/
│   └── StringResources.cs          # All UI strings — Romanian & English
│
├── Converters/
│   └── CharacterSpacingConverter.cs
│
├── Helpers/
│   └── TextBlockExtensions.cs
│
├── Assets/
│   ├── AppIcon.ico
│   └── AppIcon.png
│
└── UI/
    ├── Themes.xaml                 # Global styles, brushes, control templates
    ├── MainWindow.xaml/.cs         # Wizard shell, sidebar, step navigation
    └── Pages/
        ├── PageWelcome             # Step 1 — intro & feature overview
        ├── PageScan                # Step 2 — hardware scan & GPU tips
        ├── PageBackup              # Step 3 — create restore point & backups
        ├── PageProfiles            # Step 4 — Safe / Balanced / Gaming preset
        ├── PageOptions             # Step 5 — fine-tune individual modules
        ├── PageExecute             # Step 6 — live optimization with colored log
        └── PageDone                # Step 7 — next steps & restart
```

---

## Safety

- **No BIOS / firmware / voltage / frequency changes**
- **No driver modifications**
- All tweaks target Windows registry, power settings, and service states only
- Every module has a documented risk level (Safe / Cosmetic / Medium)
- A restore point is created before execution — you can always roll back

---

## License

MIT — free to use, modify and distribute.
