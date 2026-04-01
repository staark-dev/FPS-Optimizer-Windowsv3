namespace FPSOptimizer.Core;

public enum GpuVendor { Unknown, Nvidia, Amd, Intel }

public sealed class CpuInfo
{
    public string Name    { get; init; } = "Necunoscut";
    public int    Cores   { get; init; }
    public int    Threads { get; init; }
}

public sealed class RamInfo
{
    public double Gb    { get; init; }
    public string Speed { get; init; } = "?";
}

public sealed class GpuInfo
{
    public string    Name   { get; init; } = "Necunoscut";
    public string    Vram   { get; init; } = "?";
    public string    Driver { get; init; } = "?";
    public GpuVendor Vendor { get; init; } = GpuVendor.Unknown;

    public bool IsNvidia => Vendor == GpuVendor.Nvidia;
    public bool IsAmd    => Vendor == GpuVendor.Amd;
    public bool IsIntel  => Vendor == GpuVendor.Intel;
}

public sealed class OsInfo
{
    public string Name    { get; init; } = "Windows";
    public int    Build   { get; init; }
    public bool   IsWin11 => Build >= 22000;
}

public sealed class DiskInfo
{
    public double FreeGb  { get; init; }
    public double TotalGb { get; init; }
    public bool   IsSsd   { get; init; }
}

public sealed class SystemInfo
{
    public CpuInfo  Cpu       { get; init; } = new();
    public RamInfo  Ram       { get; init; } = new();
    public GpuInfo  Gpu       { get; init; } = new();
    public OsInfo   Os        { get; init; } = new();
    public DiskInfo Disk      { get; init; } = new();
    public string   PowerPlan { get; init; } = "Necunoscut";

    public string GpuTips => BuildGpuTips();

    private string BuildGpuTips()
    {
        var sb = new System.Text.StringBuilder();

        if (Gpu.IsNvidia)
        {
            sb.AppendLine("🟢  NVIDIA GeForce detectat — Sfaturi specifice:");
            sb.AppendLine();
            sb.AppendLine("  ✦ NVIDIA Control Panel → Manage 3D Settings → Power management mode = Prefer Maximum Performance");
            sb.AppendLine("  ✦ Low Latency Mode = Ultra  (reduce input lag in CS2, Valorant, Apex)");
            sb.AppendLine("  ✦ Dezactiveaza Vertical Sync in NVCP — activeaza-l doar in joc daca ai tearing");
            sb.AppendLine("  ✦ Texture Filtering Quality = High Performance  (+FPS, texturi usor mai putin ascutite)");
            sb.AppendLine("  ✦ Activeaza NVIDIA Reflex in jocurile compatibile  (latenta input -30% to -50%)");
            if (Gpu.Name.Contains("RTX 40") || Gpu.Name.Contains("RTX 30"))
                sb.AppendLine("  ✦ DLSS 3 Frame Generation disponibil pe RTX 40+ in jocuri compatibile — activeaza!");
            sb.AppendLine("  ✦ GeForce Experience Overlay: dezactiveaza-l  (Alt+Z → Settings → General)");
            sb.AppendLine("  ✦ Curatare driver: foloseste DDU (Display Driver Uninstaller) la reinstalare curata");
        }
        else if (Gpu.IsAmd)
        {
            sb.AppendLine("🔴  AMD Radeon detectat — Sfaturi specifice:");
            sb.AppendLine();
            sb.AppendLine("  ✦ AMD Adrenalin → Gaming → Global Graphics → Radeon Anti-Lag = Enabled");
            sb.AppendLine("  ✦ Radeon Boost: dezactiveaza pentru vizibilitate uniforma in competitive");
            sb.AppendLine("  ✦ Texture Filtering Quality = Performance");
            sb.AppendLine("  ✦ FreeSync: activeaza daca monitorul suporta (0 penalizare FPS, fara tearing)");
            sb.AppendLine("  ✦ Radeon Super Resolution (RSR): mareste FPS in jocuri fara FSR nativ");
            if (Cpu.Name.Contains("Ryzen"))
                sb.AppendLine("  ✦ Smart Access Memory (SAM/ReBAR): activeaza din BIOS → Advanced → AMD CBS  (+10-15% FPS)");
            sb.AppendLine("  ✦ Adrenalin Overlay: dezactiveaza-l daca nu ai nevoie de el");
        }
        else if (Gpu.IsIntel)
        {
            sb.AppendLine("🔵  Intel Graphics detectat — Sfaturi specifice:");
            sb.AppendLine();
            sb.AppendLine("  ✦ Intel Graphics Command Center → Gaming → reduce Texture Quality pt FPS");
            sb.AppendLine("  ✦ Asigura-te ca RAM ruleaza in Dual Channel (2 barete identice)  (+20-30% GPU integrat)");
            sb.AppendLine("  ✦ In BIOS: aloca maxim VRAM pentru grafic integrat (512MB sau 1GB daca ai optiunea)");
            sb.AppendLine("  ✦ Considerati un GPU dedicat entry-level pentru gaming serios (ex: RX 6600, RTX 3060)");
        }

        sb.AppendLine();
        sb.AppendLine("━━━  Sfaturi Generale GPU  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine();
        sb.AppendLine("  ✦ Temperatura GPU normala sub sarcina: < 85°C  |  Alarma: > 95°C → curata praful!");
        sb.AppendLine("  ✦ MSI Afterburner + RivaTuner: monitorizare FPS si temperatura in timp real (gratuit)");
        sb.AppendLine("  ✦ Display Settings → Advanced → Preferred refresh rate = Highest available");
        sb.AppendLine("  ✦ Dezactiveaza TOATE overlay-urile: Discord, Steam, Xbox, GeForce Exp → -5 to -15% FPS");
        sb.AppendLine("  ✦ Rezolutie: 1080p > 1440p > 4K pentru FPS maxim — coboara pentru competitive");
        sb.AppendLine("  ✦ Pasta termica GPU: dupa 4-5 ani, inlocuieste pasta → -10 to -15°C temperatura");
        sb.AppendLine("  ✦ Hardware-Accelerated GPU Scheduling (HAGS): activat de optimizer automat pe Win10/11");
        sb.AppendLine("  ✦ V-Sync OFF in joc + G-Sync/FreeSync = setup ideal fara tearing si fara input lag");

        return sb.ToString();
    }
}