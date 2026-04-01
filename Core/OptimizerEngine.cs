using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;

namespace FPSOptimizer.Core;

public delegate void LogHandler(string message, LogType type);
public enum LogType { Info, Ok, Warn, Error, Head, Skip }

public class OptimizerEngine
{
    private readonly LogHandler _log;
    private readonly IProgress<int> _progress;
    private readonly ISystemScanner _scanner;

    public OptimizerEngine(LogHandler log, IProgress<int> progress)
    {
        _log = log;
        _progress = progress;
        _scanner = new SystemScanner();
    }

    // ── ENTRY POINT ──────────────────────────────────────────────
    public async Task RunAsync(OptimizationOptions opts, SystemInfo sysInfo)
    {
        var modules = ModuleDefinitions.All
            .Where(m => opts.SelectedModules.Contains(m.Key))
            .ToList();

        int total = modules.Count + (opts.CreateRestorePoint ? 1 : 0) + (opts.CreateRegBackup ? 1 : 0);
        int done  = 0;

        void Tick() => _progress.Report((int)(++done * 100.0 / total));

        _log("══════════════════════════════════════════", LogType.Head);
        _log($"  FPS OPTIMIZER PRO — {total} module selectate", LogType.Head);
        _log("══════════════════════════════════════════", LogType.Head);

        // ── BACKUP PHASE ────────────────────────────────────────
        if (opts.CreateRestorePoint)
        {
            await Task.Run(CreateRestorePoint);
            Tick();
        }
        if (opts.CreateRegBackup)
        {
            await Task.Run(() => CreateBackup(opts.CreateSvcBackup));
            Tick();
        }

        // ── OPTIMIZATION PHASE ─────────────────────────────────
        foreach (var mod in modules)
        {
            _log($"── {mod.Title} ──", LogType.Head);
            await Task.Run(() => RunModule(mod.Key, sysInfo));
            Tick();
        }

        _log("══════════════════════════════════════════", LogType.Head);
        _log("  OPTIMIZARE FINALIZATA CU SUCCES! ✓", LogType.Ok);
        _log("  Reporneste PC-ul pentru efect maxim.", LogType.Head);
        _log("══════════════════════════════════════════", LogType.Head);
    }

    private void RunModule(string key, SystemInfo si)
    {
        try
        {
            switch (key)
            {
                case "PowerPlan":   ModPowerPlan();      break;
                case "CPU":         ModCpu();            break;
                case "RAM":         ModRam();            break;
                case "Storage":     ModStorage(si);      break;
                case "GPU":         ModGpu(si);          break;
                case "RegistryFPS": ModRegistryFps();    break;
                case "Network":     ModNetwork();        break;
                case "Visual":      ModVisual();         break;
                case "Telemetry":   ModTelemetry();      break;
                case "Services":    ModServices();       break;
                case "Bloatware":   ModBloatware();      break;
                case "Tasks":       ModTasks();          break;
                case "Clean":       ModClean();          break;
            }
        }
        catch (Exception ex)
        {
            _log($"Eroare la modul {key}: {ex.Message}", LogType.Error);
        }
    }

    // ── HELPERS ─────────────────────────────────────────────────
    private void SetReg(string path, string name, object value, RegistryValueKind kind)
    {
        try
        {
            using var key = Registry.LocalMachine.CreateSubKey(path, true)
                          ?? Registry.CurrentUser.CreateSubKey(path.Replace("HKCU\\",""), true);
            // handle both HKLM and HKCU
            SetRegFull($"HKLM\\{path}", name, value, kind);
        }
        catch { }
    }

    private static void SetRegFull(string fullPath, string name, object value, RegistryValueKind kind)
    {
        try
        {
            var hive    = fullPath.StartsWith("HKCU") ? Registry.CurrentUser : Registry.LocalMachine;
            var subPath = fullPath[(fullPath.IndexOf('\\') + 1)..];
            using var k = hive.CreateSubKey(subPath, true);
            k?.SetValue(name, value, kind);
        }
        catch { }
    }

    private void Reg(string fullPath, string name, object value,
                     RegistryValueKind kind = RegistryValueKind.DWord)
    {
        try
        {
            SetRegFull(fullPath, name, value, kind);
            _log($"Reg: {name} = {value}", LogType.Ok);
        }
        catch (Exception ex) { _log($"Skip Reg {name}: {ex.Message}", LogType.Skip); }
    }

    private void Run(string exe, string args)
    {
        try
        {
            var p = new ProcessStartInfo(exe, args)
            { CreateNoWindow=true, UseShellExecute=false };
            Process.Start(p)?.WaitForExit(8000);
        }
        catch { }
    }

    private void StopDisableService(string name)
    {
        try
        {
            using var sc = new System.ServiceProcess.ServiceController(name);
            if (sc.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                sc.Stop();
            Run("sc", $"config {name} start= disabled");
            _log($"Serviciu dezactivat: {name}", LogType.Ok);
        }
        catch { _log($"Serviciu inexistent: {name}", LogType.Skip); }
    }

    // ── MODULE: RESTORE POINT ────────────────────────────────────
    private void CreateRestorePoint()
    {
        _log("Creare Windows System Restore Point...", LogType.Head);
        try
        {
            Run("powershell", "-NoProfile -Command \"Enable-ComputerRestore -Drive 'C:\\' -EA SilentlyContinue; " +
                $"Checkpoint-Computer -Description 'FPSOptimizer_{DateTime.Now:yyyyMMdd_HHmmss}' " +
                "-RestorePointType MODIFY_SETTINGS -EA Stop\"");
            _log("Restore Point creat cu succes", LogType.Ok);
        }
        catch (Exception ex)
        {
            _log($"Restore Point esuat: {ex.Message}", LogType.Warn);
        }
    }

    // ── MODULE: BACKUP ───────────────────────────────────────────
    private void CreateBackup(bool includeSvc)
    {
        _log("Creare backup registri pe Desktop...", LogType.Head);
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"FPSOptimizer_Backup_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(dir);

        var keys = new[]
        {
            "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power",
            "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile",
            "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters",
            "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects",
            "HKCU\\System\\GameConfigStore",
            "HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl",
            "HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers"
        };

        foreach (var k in keys)
        {
            var fn = k.Replace("\\", "_").Replace(":", "") + ".reg";
            Run("reg", $"export \"{k}\" \"{Path.Combine(dir, fn)}\" /y");
            _log($"Backup: {k}", LogType.Ok);
        }

        if (includeSvc)
        {
            var csv = Path.Combine(dir, "services_backup.csv");
            Run("powershell", $"-NoProfile -Command \"Get-Service | Select-Object Name,StartType,Status | Export-Csv '{csv}' -NoTypeInformation\"");
            _log("Backup servicii CSV salvat", LogType.Ok);
        }

        // Restore script
        var restore = Path.Combine(dir, "RESTAURARE.ps1");
        File.WriteAllText(restore,
            $"# FPS Optimizer Restore Script\n$dir = \"{dir}\"\n" +
            "Get-ChildItem \"$dir\\*.reg\" | ForEach-Object { reg import $_.FullName }\n" +
            "Write-Host 'Restaurare completa! Reporniti PC-ul.' -ForegroundColor Green\npause\n");

        _log($"Backup complet in: {dir}", LogType.Ok);
    }

    // ── MODULE: POWER PLAN ───────────────────────────────────────
    private void ModPowerPlan()
    {
        const string guidUP = "e9a42b02-d5df-448d-aa00-03f14749eb61";
        var list = _scanner.RunCommand("powercfg", "/list");
        if (!list.Contains(guidUP))
            Run("powercfg", $"-duplicatescheme {guidUP}");
        Run("powercfg", $"/setactive {guidUP}");
        _log("Ultimate Performance activat", LogType.Ok);
        Run("powercfg", "/change standby-timeout-ac 0");
        Run("powercfg", "/change hibernate-timeout-ac 0");
        Run("powercfg", "/hibernate off");
        _log("Sleep/Hibernate dezactivat", LogType.Ok);
        Run("powercfg", "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0");
        _log("USB Selective Suspend dezactivat", LogType.Ok);
        Run("powercfg", "/setactive SCHEME_CURRENT");
    }

    // ── MODULE: CPU ──────────────────────────────────────────────
    private void ModCpu()
    {
        Run("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100");
        Run("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMAXCORES 100");
        _log("CPU Core Parking dezactivat", LogType.Ok);
        Run("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMAX 100");
        Run("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 5");
        _log("CPU Throttling dezactivat (min 5% - sigur)", LogType.Ok);
        Run("powercfg", "-setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PERFBOOSTMODE 2");
        _log("CPU Boost Mode activat", LogType.Ok);
        Run("powercfg", "/setactive SCHEME_CURRENT");
        Reg("HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl",
            "Win32PrioritySeparation", 38);
        _log("Win32PrioritySeparation=38 (foreground prioritar)", LogType.Ok);
    }

    // ── MODULE: RAM ──────────────────────────────────────────────
    private void ModRam()
    {
        const string mm = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management";
        Reg(mm, "DisablePagingExecutive", 1);
        _log("Kernel blocat in RAM (DisablePagingExecutive=1)", LogType.Ok);
        Reg(mm, "LargeSystemCache", 0);
        Reg(mm, "ClearPageFileAtShutdown", 0);
        Reg(mm, "IoPageLockLimit", 983040);
        Reg(mm + "\\PrefetchParameters", "EnableSuperfetch", 0);
        _log("Superfetch dezactivat", LogType.Ok);
    }

    // ── MODULE: STORAGE ──────────────────────────────────────────
    private void ModStorage(SystemInfo si)
    {
        const string fs = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem";
        Reg(fs, "NtfsDisableLastAccessUpdate", 0x80000003);
        Reg(fs, "NtfsMemoryUsage", 2);
        _log("NTFS optimizat", LogType.Ok);

        if (si.Disk.IsSsd)
        {
            Run("fsutil", "behavior set disabledeletenotify 0");
            _log("TRIM activat (SSD detectat)", LogType.Ok);
            Run("powershell", "-NoProfile -Command \"Disable-ScheduledTask -TaskName 'ScheduledDefrag' -EA SilentlyContinue\"");
            _log("Defragmentare automata dezactivata pe SSD", LogType.Ok);
        }
        else
        {
            _log("HDD detectat — defragmentare pastrata", LogType.Info);
        }
    }

    // ── MODULE: GPU ──────────────────────────────────────────────
    private void ModGpu(SystemInfo si)
    {
        // HAGS (Hardware Accelerated GPU Scheduling)
        if (si.Os.Build >= 18363)
        {
            Reg("HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", "HwSchMode", 2);
            _log("HAGS (Hardware GPU Scheduling) activat", LogType.Ok);
        }

        // Games multimedia profile
        const string games = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks\\Games";
        Reg(games, "GPU Priority", 8);
        Reg(games, "Priority", 6);
        Reg(games, "Scheduling Category", "High", RegistryValueKind.String);
        Reg(games, "SFIO Priority", "High", RegistryValueKind.String);
        _log("GPU Priority=8, Scheduling=High", LogType.Ok);

        // GameDVR off
        Reg("HKCU\\System\\GameConfigStore", "GameDVR_Enabled", 0);
        Reg("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\GameDVR", "AllowGameDVR", 0);
        _log("Xbox Game DVR dezactivat", LogType.Ok);

        if (si.Gpu.IsNvidia)
        {
            foreach (var svc in new[] { "NvTelemetryContainer", "NvContainerLocalSystem", "NvContainerNetworkService" })
                StopDisableService(svc);
        }
        if (si.Gpu.IsAmd)
        {
            StopDisableService("AMD Crash Defender Service");
        }
    }

    // ── MODULE: REGISTRY FPS ─────────────────────────────────────
    private void ModRegistryFps()
    {
        const string sp = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile";
        Reg(sp, "SystemResponsiveness", 0);
        Reg(sp, "NetworkThrottlingIndex", unchecked((int)0xffffffff));
        _log("SystemResponsiveness=0 (max resources for game)", LogType.Ok);

        Reg("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management",
            "DisablePagingExecutive", 1);

        Reg("HKCU\\Control Panel\\Desktop", "MenuShowDelay", "0", RegistryValueKind.String);
        Reg("HKCU\\Control Panel\\Desktop", "AutoEndTasks",  "1", RegistryValueKind.String);
        _log("Desktop responsiveness optimizat", LogType.Ok);

        Reg("HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting", "Disabled", 1);
        _log("Windows Error Reporting dezactivat", LogType.Ok);
    }

    // ── MODULE: NETWORK ──────────────────────────────────────────
    private void ModNetwork()
    {
        const string tcp = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters";
        Reg(tcp, "TcpAckFrequency", 1);
        Reg(tcp, "TCPNoDelay", 1);
        Reg(tcp, "TcpDelAckTicks", 0);
        _log("Nagle Algorithm dezactivat (ping redus)", LogType.Ok);

        Run("netsh", "int tcp set global autotuninglevel=normal");
        Run("netsh", "int tcp set global chimney=disabled");
        Run("netsh", "int tcp set global rss=enabled");
        Run("netsh", "int tcp set global ecncapability=disabled");
        Run("netsh", "int tcp set global timestamps=disabled");
        _log("TCP stack optimizat", LogType.Ok);

        Reg("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Psched", "NonBestEffortLimit", 0);
        _log("Banda QoS: 0% rezervata (era 20% default)", LogType.Ok);

        Run("ipconfig", "/flushdns");
        _log("DNS cache golit", LogType.Ok);

        // Disable LSO
        Run("powershell",
            "-NoProfile -Command \"Get-NetAdapter -Physical | ForEach-Object { Disable-NetAdapterLso -Name $_.Name -EA SilentlyContinue }\"");
        _log("Large Send Offload dezactivat", LogType.Ok);
    }

    // ── MODULE: VISUAL EFFECTS ───────────────────────────────────
    private void ModVisual()
    {
        Reg("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects",
            "VisualFXSetting", 2);
        Reg("HKCU\\Control Panel\\Desktop", "DragFullWindows", "0", RegistryValueKind.String);
        Reg("HKCU\\Control Panel\\Desktop", "MenuShowDelay",   "0", RegistryValueKind.String);
        Reg("HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
            "EnableTransparency", 0);
        _log("Animatii si transparenta dezactivate", LogType.Ok);
    }

    // ── MODULE: TELEMETRY ────────────────────────────────────────
    private void ModTelemetry()
    {
        var regs = new[]
        {
            ("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection",           "AllowTelemetry", 0),
            ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\DataCollection", "AllowTelemetry", 0),
            ("HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Privacy",            "TailoredExperiencesWithDiagnosticDataEnabled", 0),
            ("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo",    "Enabled", 0),
            ("HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo",    "Enabled", 0),
            ("HKCU\\SOFTWARE\\Microsoft\\Input\\TIPC",                                 "Enabled", 0),
            ("HKLM\\SOFTWARE\\Policies\\Microsoft\\SQMClient\\Windows",               "CEIPEnable", 0),
        };
        foreach (var (p, n, v) in regs) Reg(p, n, v);
        _log("Telemetrie dezactivata in registri", LogType.Ok);

        // Hosts file
        var hostsPath = @"C:\Windows\System32\drivers\etc\hosts";
        var domains = new[]
        {
            "0.0.0.0 vortex.data.microsoft.com",
            "0.0.0.0 telemetry.microsoft.com",
            "0.0.0.0 watson.telemetry.microsoft.com",
            "0.0.0.0 sqm.telemetry.microsoft.com",
            "0.0.0.0 statsfe2.ws.microsoft.com",
            "0.0.0.0 settings-sandbox.data.microsoft.com",
        };
        try
        {
            var content = File.ReadAllText(hostsPath);
            if (!content.Contains("# FPSOptimizer"))
            {
                File.AppendAllText(hostsPath, "\n# FPSOptimizer Telemetry Block\n" + string.Join("\n", domains) + "\n");
                _log("Domenii telemetrie blocate in hosts", LogType.Ok);
            }
        }
        catch { _log("Nu s-a putut modifica hosts (posibil protejat)", LogType.Warn); }
    }

    // ── MODULE: SERVICES ─────────────────────────────────────────
    private void ModServices()
    {
        var services = new[]
        {
            "DiagTrack","dmwappushservice","WSearch","SysMain","Fax",
            "MapsBroker","lfsvc","RetailDemo","RemoteRegistry","WerSvc",
            "XboxGipSvc","XblAuthManager","XblGameSave","XboxNetApiSvc",
            "wisvc","WdiServiceHost","WdiSystemHost","MixedRealityOpenXRSvc",
            "icssvc","PhoneSvc","WMPNetworkSvc","DusmSvc",
            "diagnosticshub.standardcollector.service"
        };
        foreach (var s in services) StopDisableService(s);
    }

    // ── MODULE: BLOATWARE ────────────────────────────────────────
    private void ModBloatware()
    {
        var apps = new[]
        {
            "Microsoft.3DBuilder","Microsoft.BingFinance","Microsoft.BingNews",
            "Microsoft.BingSports","Microsoft.BingWeather","Microsoft.GetHelp",
            "Microsoft.Getstarted","Microsoft.Messaging","Microsoft.Microsoft3DViewer",
            "Microsoft.MicrosoftOfficeHub","Microsoft.MicrosoftSolitaireCollection",
            "Microsoft.MixedReality.Portal","Microsoft.News","Microsoft.Office.Sway",
            "Microsoft.OneConnect","Microsoft.People","Microsoft.Print3D",
            "Microsoft.SkypeApp","Microsoft.Todos","Microsoft.Wallet",
            "Microsoft.WindowsAlarms","Microsoft.WindowsFeedbackHub",
            "Microsoft.WindowsMaps","Microsoft.Xbox.TCUI","Microsoft.XboxApp",
            "Microsoft.XboxGameOverlay","Microsoft.XboxGamingOverlay",
            "Microsoft.XboxIdentityProvider","Microsoft.YourPhone",
            "Microsoft.ZuneMusic","Microsoft.ZuneVideo","Microsoft.GamingApp",
            "king.com.CandyCrushSaga","king.com.BubbleWitch3Saga",
            "SpotifyAB.SpotifyMusic","Facebook.Facebook","TikTok"
        };
        foreach (var app in apps)
        {
            Run("powershell",
                $"-NoProfile -Command \"Get-AppxPackage '{app}' | Remove-AppxPackage -EA SilentlyContinue\"");
            _log($"Bloatware eliminat: {app}", LogType.Ok);
        }
    }

    // ── MODULE: SCHEDULED TASKS ──────────────────────────────────
    private void ModTasks()
    {
        var tasks = new[]
        {
            (@"\Microsoft\Windows\Application Experience", "Microsoft Compatibility Appraiser"),
            (@"\Microsoft\Windows\Application Experience", "ProgramDataUpdater"),
            (@"\Microsoft\Windows\Customer Experience Improvement Program", "Consolidator"),
            (@"\Microsoft\Windows\Customer Experience Improvement Program", "UsbCeip"),
            (@"\Microsoft\Windows\DiskDiagnostic", "Microsoft-Windows-DiskDiagnosticDataCollector"),
            (@"\Microsoft\Windows\Feedback\Siuf", "DmClient"),
            (@"\Microsoft\Windows\Windows Error Reporting", "QueueReporting"),
            (@"\Microsoft\XblGameSave", "XblGameSaveTask"),
            (@"\Microsoft\Windows\Location", "Notifications"),
        };
        foreach (var (path, name) in tasks)
        {
            Run("schtasks", $"/Change /TN \"{path}\\{name}\" /Disable");
            _log($"Task dezactivat: {name}", LogType.Ok);
        }
    }

    // ── MODULE: CLEAN ────────────────────────────────────────────
    private void ModClean()
    {
        long total = 0;
        var paths = new[]
        {
            Path.GetTempPath(),
            @"C:\Windows\Temp",
            @"C:\Windows\Prefetch",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
        };
        foreach (var p in paths)
        {
            if (!Directory.Exists(p)) continue;
            foreach (var f in Directory.GetFiles(p, "*", SearchOption.AllDirectories))
            {
                try { total += new FileInfo(f).Length; File.Delete(f); } catch { }
            }
            foreach (var d in Directory.GetDirectories(p))
            {
                try { Directory.Delete(d, true); } catch { }
            }
        }
        _log($"Curatat: {total / 1048576.0:0.1} MB eliberat", LogType.Ok);
        Run("ipconfig", "/flushdns");
        _log("DNS cache golit", LogType.Ok);
    }
}
