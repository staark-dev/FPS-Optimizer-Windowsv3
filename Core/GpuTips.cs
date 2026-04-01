using System.Text;

namespace FPSOptimizer.Core;

// ── Contract ─────────────────────────────────────────────────────────────────

public interface IGpuTipsProvider
{
    bool CanHandle(GpuInfo gpu);
    void AppendVendorTips(StringBuilder sb, SystemInfo system);
}

// ── Builder ──────────────────────────────────────────────────────────────────

public sealed class GpuTipsBuilder
{
    private readonly IReadOnlyList<IGpuTipsProvider> _providers;

    /// Constructor implicit — providers înregistrați în ordinea priorității.
    public GpuTipsBuilder()
        : this([new NvidiaGpuTipsProvider(), new AmdGpuTipsProvider(), new IntelGpuTipsProvider()]) { }

    /// Constructor DI — injectează providers custom sau mock-uri pentru teste.
    public GpuTipsBuilder(IEnumerable<IGpuTipsProvider> providers)
        => _providers = providers.ToList().AsReadOnly();

    public string Build(SystemInfo system)
    {
        var sb = new StringBuilder();

        _providers.FirstOrDefault(p => p.CanHandle(system.Gpu))
                  ?.AppendVendorTips(sb, system);

        AppendGeneralTips(sb);
        return sb.ToString();
    }

    private static void AppendGeneralTips(StringBuilder sb)
    {
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
    }
}

// ── Providers ────────────────────────────────────────────────────────────────

public sealed class NvidiaGpuTipsProvider : IGpuTipsProvider
{
    public bool CanHandle(GpuInfo gpu) => gpu.IsNvidia;

    public void AppendVendorTips(StringBuilder sb, SystemInfo system)
    {
        sb.AppendLine("🟢  NVIDIA GeForce detectat — Sfaturi specifice:");
        sb.AppendLine();
        sb.AppendLine("  ✦ NVIDIA Control Panel → Manage 3D Settings → Power management mode = Prefer Maximum Performance");
        sb.AppendLine("  ✦ Low Latency Mode = Ultra  (reduce input lag in CS2, Valorant, Apex)");
        sb.AppendLine("  ✦ Dezactiveaza Vertical Sync in NVCP — activeaza-l doar in joc daca ai tearing");
        sb.AppendLine("  ✦ Texture Filtering Quality = High Performance  (+FPS, texturi usor mai putin ascutite)");
        sb.AppendLine("  ✦ Activeaza NVIDIA Reflex in jocurile compatibile  (latenta input -30% to -50%)");

        if (system.Gpu.Name.Contains("RTX 40", StringComparison.OrdinalIgnoreCase) ||
            system.Gpu.Name.Contains("RTX 50", StringComparison.OrdinalIgnoreCase))
            sb.AppendLine("  ✦ DLSS 4 Multi Frame Generation disponibil pe RTX 40/50 in jocuri compatibile — activeaza!");
        else if (system.Gpu.Name.Contains("RTX 30", StringComparison.OrdinalIgnoreCase))
            sb.AppendLine("  ✦ DLSS 3 Frame Generation disponibil pe RTX 40+ — upgrade recomandat pentru FPS dublu");

        sb.AppendLine("  ✦ GeForce Experience Overlay: dezactiveaza-l  (Alt+Z → Settings → General)");
        sb.AppendLine("  ✦ Curatare driver: foloseste DDU (Display Driver Uninstaller) la reinstalare curata");
    }
}

public sealed class AmdGpuTipsProvider : IGpuTipsProvider
{
    public bool CanHandle(GpuInfo gpu) => gpu.IsAmd;

    public void AppendVendorTips(StringBuilder sb, SystemInfo system)
    {
        sb.AppendLine("🔴  AMD Radeon detectat — Sfaturi specifice:");
        sb.AppendLine();
        sb.AppendLine("  ✦ AMD Adrenalin → Gaming → Global Graphics → Radeon Anti-Lag = Enabled");
        sb.AppendLine("  ✦ Radeon Boost: dezactiveaza pentru vizibilitate uniforma in competitive");
        sb.AppendLine("  ✦ Texture Filtering Quality = Performance");
        sb.AppendLine("  ✦ FreeSync: activeaza daca monitorul suporta (0 penalizare FPS, fara tearing)");
        sb.AppendLine("  ✦ Radeon Super Resolution (RSR): mareste FPS in jocuri fara FSR nativ");

        if (system.Cpu.Name.Contains("Ryzen", StringComparison.OrdinalIgnoreCase))
            sb.AppendLine("  ✦ Smart Access Memory (SAM/ReBAR): activeaza din BIOS → Advanced → AMD CBS  (+10-15% FPS)");

        sb.AppendLine("  ✦ Adrenalin Overlay: dezactiveaza-l daca nu ai nevoie de el");
    }
}

public sealed class IntelGpuTipsProvider : IGpuTipsProvider
{
    public bool CanHandle(GpuInfo gpu) => gpu.IsIntel;

    public void AppendVendorTips(StringBuilder sb, SystemInfo system)
    {
        sb.AppendLine("🔵  Intel Graphics detectat — Sfaturi specifice:");
        sb.AppendLine();
        sb.AppendLine("  ✦ Intel Graphics Command Center → Gaming → reduce Texture Quality pt FPS");
        sb.AppendLine("  ✦ Asigura-te ca RAM ruleaza in Dual Channel (2 barete identice)  (+20-30% GPU integrat)");
        sb.AppendLine("  ✦ In BIOS: aloca maxim VRAM pentru grafic integrat (512MB sau 1GB daca ai optiunea)");
        sb.AppendLine("  ✦ Considerati un GPU dedicat entry-level pentru gaming serios (ex: RX 7600, RTX 4060)");
    }
}