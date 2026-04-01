namespace FPSOptimizer.Core;

using FPSOptimizer.Localization;

public enum RiskLevel { Safe, Cosmetic, Medium }

public class OptimizationModule
{
    public string   Key      { get; init; } = "";
    public string   Category { get; init; } = "";
    public string   Icon     { get; init; } = "";
    public string   Title    { get; init; } = "";
    public string   Desc     { get; init; } = "";
    public string   Details  { get; init; } = "";
    public RiskLevel Risk    { get; init; } = RiskLevel.Safe;
    public bool     Default  { get; init; } = true;

    public string RiskLabel => Risk switch
    {
        RiskLevel.Safe     => "✓  Sigur",
        RiskLevel.Cosmetic => "◆  Cosmetic",
        RiskLevel.Medium   => "▲  Mediu",
        _ => "?"
    };
    public System.Windows.Media.Brush RiskColor => Risk switch
    {
        RiskLevel.Safe     => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 200, 100)),
        RiskLevel.Cosmetic => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 50)),
        RiskLevel.Medium   => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 160, 30)),
        _ => System.Windows.Media.Brushes.Gray
    };
}

public class OptimizationOptions
{
    public bool CreateRestorePoint { get; set; } = true;
    public bool CreateRegBackup    { get; set; } = true;
    public bool CreateSvcBackup    { get; set; } = true;
    public HashSet<string> SelectedModules { get; set; } = new(
        ModuleDefinitions.All.Where(m => m.Default).Select(m => m.Key));
    public Language Language { get; set; } = Language.RO;
}

public static class ModuleDefinitions
{
    public static readonly List<OptimizationModule> All = new()
    {
        new() { Key="PowerPlan",   Category="⚡  Energie & CPU",         Icon="⚡",
            Title="Plan Energie Ultimate Performance",
            Desc ="Activeaza planul Microsoft Ultimate Performance. Dezactiveaza sleep, hibernate, USB suspend.",
            Details="Modifica: powercfg. NU atinge BIOS sau firmware.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="CPU",         Category="⚡  Energie & CPU",         Icon="🔲",
            Title="Optimizare Procesor",
            Desc ="Dezactiveaza Core Parking (toate nucleele active). Activeaza Boost Mode maxim. Priority boost foreground.",
            Details="Modifica: powercfg setacvalueindex, HKLM PriorityControl. NU schimba frecvente sau voltaje.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="RAM",         Category="💾  Memorie & Stocare",     Icon="💾",
            Title="Optimizare Memorie RAM",
            Desc ="Blocheaza kernelul Windows in RAM (reduce page faults). Optimizeaza IoPageLockLimit.",
            Details="Modifica: HKLM Memory Management — DisablePagingExecutive=1, LargeSystemCache=0.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="Storage",     Category="💾  Memorie & Stocare",     Icon="💿",
            Title="Optimizare Stocare SSD/HDD",
            Desc ="Detecteaza automat SSD/HDD. TRIM activat pe SSD. Dezactiveaza NTFS Last Access Update.",
            Details="Dezactiveaza defrag automat pe SSD. NTFS: NtfsDisableLastAccessUpdate=1.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="GPU",         Category="🎮  Placa Video",           Icon="🎮",
            Title="Optimizare GPU & DirectX",
            Desc ="GPU Priority=8, Hardware GPU Scheduling activat, GameDVR dezactivat. Telemetrie NVIDIA/AMD oprita.",
            Details="NU face overclocking. Modifica doar registri Windows si servicii driver. Sigur 100%.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="RegistryFPS", Category="🎮  Placa Video",           Icon="🔧",
            Title="Registry Tweaks FPS",
            Desc ="SystemResponsiveness=0, NetworkThrottlingIndex=max, Win32PrioritySeparation optimizat pentru jocuri.",
            Details="Tweaks verificate, folosite de milioane de gameri. Backup se face inainte obligatoriu.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="Network",     Category="🌐  Rețea",                 Icon="🌐",
            Title="Optimizare Rețea (Latenta Minima)",
            Desc ="Dezactiveaza Nagle Algorithm (ping mai mic). TCP optimizat. Elibereaza 20% banda rezervata QoS.",
            Details="Modifica: registri TCP, netsh int tcp. NU schimba IP, DNS sau drivere de retea.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="Visual",      Category="👁️  Interfata",             Icon="👁️",
            Title="Efecte Vizuale — Performanta Maxima",
            Desc ="Dezactiveaza animatii Windows, transparenta si efecte vizuale care consuma GPU/CPU inutil.",
            Details="Reversibil oricand din System Properties → Performance → Visual Effects.",
            Risk=RiskLevel.Cosmetic, Default=true },

        new() { Key="Telemetry",   Category="🛡️  Confidentialitate",     Icon="🛡️",
            Title="Dezactivare Telemetrie & Tracking",
            Desc ="Dezactiveaza colectarea de date Microsoft. Blocheaza domeniile de telemetrie in hosts file.",
            Details="Modifica: HKLM DataCollection, fisier hosts. NU afecteaza Windows Update.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="Services",    Category="⚙️  Servicii",              Icon="⚙️",
            Title="Dezactivare Servicii Inutile",
            Desc ="Opreste 23 servicii de fundal: Xbox Live, telemetrie, Fax, Mixed Reality etc.",
            Details="Selectie atenta — fara servicii critice. Backup CSV al serviciilor inclus.",
            Risk=RiskLevel.Medium, Default=true },

        new() { Key="Bloatware",   Category="🗑️  Aplicatii",            Icon="🗑️",
            Title="Eliminare Bloatware Preinstalat",
            Desc ="Sterge 35+ apps inutile: Candy Crush, Xbox apps, TikTok, Bing apps etc.",
            Details="Reinstalabile din Microsoft Store oricand. NU sterge Office sau apps utile.",
            Risk=RiskLevel.Medium, Default=true },

        new() { Key="Tasks",       Category="🗑️  Aplicatii",            Icon="📋",
            Title="Dezactivare Taskuri Planificate",
            Desc ="Dezactiveaza taskuri CEIP, Compatibility Appraiser, Xbox Game Save, telemetrie.",
            Details="Taskuri de fundal inutile. Reversibil din Task Scheduler.",
            Risk=RiskLevel.Safe, Default=true },

        new() { Key="Clean",       Category="🧹  Curatare",              Icon="🧹",
            Title="Curatare Fisiere Temporare",
            Desc ="Sterge %TEMP%, Windows\\Temp, Prefetch. Goleste DNS cache si WinSxS temp.",
            Details="100% sigur. Fisierele temporare sunt recreate automat de Windows.",
            Risk=RiskLevel.Safe, Default=true },
    };
}
