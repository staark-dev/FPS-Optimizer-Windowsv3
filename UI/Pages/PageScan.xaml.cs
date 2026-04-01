using FPSOptimizer.Core;
using FPSOptimizer.Localization;
using System.Windows.Controls;
using System.Windows.Media;

namespace FPSOptimizer.UI.Pages;

public partial class PageScan : Page
{
    private SystemInfo? _lastInfo;

    public PageScan() => InitializeComponent();

    public void UpdateLanguage()
    {
        var S = StringResources.Get;
        TxtScanTitle.Text    = S("Scan.Title");
        TxtScanDesc.Text     = S("Scan.Desc");
        LblCpuHeader.Text    = S("Scan.CPU");
        LblRamHeader.Text    = S("Scan.RAM");
        LblGpuHeader.Text    = S("Scan.GPU");
        LblOsHeader.Text     = S("Scan.OS");
        LblDiskHeader.Text   = S("Scan.Disk");
        LblPowerHeader.Text  = S("Scan.Power");
        LblPowerSub.Text     = S("Scan.PowerSub");
        TxtTipsHeader.Text   = S("Scan.Tips");

        if (GpuTipsText.Text == StringResources.RO["Scan.TipsLoading"] ||
            GpuTipsText.Text == StringResources.EN["Scan.TipsLoading"])
            GpuTipsText.Text = S("Scan.TipsLoading");

        // Re-populate dynamic texts if scan was already done
        if (_lastInfo != null)
            PopulateInfo(_lastInfo);
        else
        {
            var scanning = S("Scan.Scanning");
            LblCpu.Text = LblRam.Text = LblGpu.Text = LblOs.Text = LblDisk.Text = LblPower.Text = scanning;
        }
    }

    public void PopulateInfo(SystemInfo si)
    {
        _lastInfo = si;
        var S = StringResources.Get;

        LblCpu.Text    = si.Cpu.Name;
        LblCpuSub.Text = string.Format(S("Scan.CpuSub"), si.Cpu.Cores, si.Cpu.Threads);

        LblRam.Text    = $"{si.Ram.Gb:0.#} GB";
        LblRamSub.Text = $"Speed: {si.Ram.Speed}";

        LblGpu.Text    = si.Gpu.Name;
        LblGpuSub.Text = $"VRAM: {si.Gpu.Vram}  |  Driver: {si.Gpu.Driver}";

        LblOs.Text     = si.Os.Name;
        LblOsSub.Text  = string.Format(S("Scan.OsBuild"), si.Os.Build,
                            si.Os.IsWin11 ? S("Scan.Win11") : S("Scan.Win10"));

        LblDisk.Text    = string.Format(S("Scan.DiskFree"), si.Disk.FreeGb, si.Disk.TotalGb);
        LblDiskSub.Text = si.Disk.IsSsd ? S("Scan.DiskSsd") : S("Scan.DiskHdd");
        LblDiskSub.Foreground = si.Disk.IsSsd
            ? new SolidColorBrush(Color.FromRgb(0, 230, 130))
            : new SolidColorBrush(Color.FromRgb(140, 145, 175));

        LblPower.Text = si.PowerPlan;
        LblPower.Foreground = si.PowerPlan.Contains("Ultimate")
            ? new SolidColorBrush(Color.FromRgb(0, 230, 130))
            : new SolidColorBrush(Color.FromRgb(255, 200, 50));

        GpuTipsText.Text = si.GpuTips;
    }
}
