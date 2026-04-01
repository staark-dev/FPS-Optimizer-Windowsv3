using FPSOptimizer.Localization;
using System.Windows;
using System.Windows.Controls;

namespace FPSOptimizer.UI.Pages;

public partial class PageDone : Page
{
    public PageDone()
    {
        InitializeComponent();
        UpdateLanguage();
    }

    public void UpdateLanguage()
    {
        var S = StringResources.Get;
        TxtDoneTitle.Text    = S("Done.Title");
        TxtDoneSubtitle.Text = S("Done.Subtitle");
        TxtNextSteps.Text    = S("Done.NextSteps");
        TxtStep1Badge.Text   = S("Done.Step1Badge");
        TxtStep1Title.Text   = S("Done.Step1Title");
        TxtStep1Desc.Text    = S("Done.Step1Desc");
        TxtGpuTitle.Text     = S("Done.GpuTitle");
        TxtGpuNvidia.Text    = S("Done.GpuNvidia");
        TxtGpuNvidiaDesc.Text= S("Done.GpuNvidiaDesc");
        TxtGpuAmd.Text       = S("Done.GpuAmd");
        TxtGpuAmdDesc.Text   = S("Done.GpuAmdDesc");
        TxtOverlayTitle.Text = S("Done.OverlayTitle");
        TxtOverlayDesc.Text  = S("Done.OverlayDesc");
        TxtMonitorTitle.Text = S("Done.MonitorTitle");
        TxtMonitorDesc.Text  = S("Done.MonitorDesc");
        TxtRestoreTitle.Text = S("Done.RestoreTitle");
        TxtRestoreDesc.Text  = S("Done.RestoreDesc");
        TxtMaintTitle.Text   = S("Done.MaintTitle");
        TxtMaint1.Text       = S("Done.Maint1");
        TxtMaint2.Text       = S("Done.Maint2");
        TxtMaint3.Text       = S("Done.Maint3");
        TxtMaint4.Text       = S("Done.Maint4");
        BtnRestart.Content   = S("Done.RestartBtn");
    }

    private void BtnRestart_Click(object sender, RoutedEventArgs e)
    {
        var S = StringResources.Get;
        var r = MessageBox.Show(S("Done.RestartConfirm"), S("Done.RestartTitle"),
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Yes)
            System.Diagnostics.Process.Start("shutdown",
                $"/r /t 5 /c \"{S("Done.RestartCmd")}\"");
    }
}
