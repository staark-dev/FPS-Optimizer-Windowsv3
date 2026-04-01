using FPSOptimizer.Localization;
using FPSOptimizer.UI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace FPSOptimizer.UI.Pages;

public partial class PageBackup : Page
{
    private bool _restorePointCreated = false;

    public PageBackup()
    {
        InitializeComponent();
        ChkRestorePoint.Checked   += UpdateWarning;
        ChkRestorePoint.Unchecked += UpdateWarning;
        ChkRegBackup.Checked      += UpdateWarning;
        ChkRegBackup.Unchecked    += UpdateWarning;
        ChkSvcBackup.Checked      += UpdateWarning;
        ChkSvcBackup.Unchecked    += UpdateWarning;
        UpdateLanguage();
    }

    public void UpdateLanguage()
    {
        var S = StringResources.Get;
        TxtTitle.Text         = S("Backup.Title");
        TxtDesc.Text          = S("Backup.Desc");
        TxtRPTitle.Text       = S("Backup.RestorePoint");
        TxtRPBadge.Text       = S("Backup.RPBadge");
        TxtRPDesc.Text        = S("Backup.RPDesc");
        TxtRegTitle.Text      = S("Backup.RegTitle");
        TxtRegDesc.Text       = S("Backup.RegDesc");
        TxtRegRisk.Text       = S("Backup.Risk.Low");
        TxtSvcTitle.Text      = S("Backup.SvcTitle");
        TxtSvcDesc.Text       = S("Backup.SvcDesc");
        TxtSvcRisk.Text       = S("Backup.Risk.Low");
        TxtWarnTitle.Text     = S("Backup.WarnTitle");
        TxtWarnMsg.Text       = S("Backup.WarnMsg");
        TxtOkTitle.Text       = S("Backup.OkTitle");
        TxtOkMsg.Text         = S("Backup.OkMsg");

        if (!_restorePointCreated)
            BtnCreateRestore.Content = S("Backup.RPBtn");
    }

    private void UpdateWarning(object sender, RoutedEventArgs e)
    {
        bool anyChecked = ChkRestorePoint.IsChecked == true
                       || ChkRegBackup.IsChecked    == true
                       || ChkSvcBackup.IsChecked    == true;
        WarnBox.Visibility = anyChecked ? Visibility.Collapsed : Visibility.Visible;
        OkBox.Visibility   = anyChecked ? Visibility.Visible   : Visibility.Collapsed;
    }

    private async void BtnCreateRestore_Click(object sender, RoutedEventArgs e)
    {
        var S = StringResources.Get;

        if (ChkRestorePoint.IsChecked != true)
        {
            MessageBox.Show(S("Backup.RPNotChecked"), S("Options.Warning"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BtnCreateRestore.IsEnabled    = false;
        BtnCreateRestore.Content      = S("Backup.RPCreating");
        RestorePointStatus.Text       = "⏳ " + S("Backup.RPCreating");
        RestorePointResult.Visibility = Visibility.Collapsed;

        var (success, errorMsg) = await Task.Run(CreateRestorePoint);

        if (success)
        {
            BtnCreateRestore.Content      = S("Backup.RPCreated");
            RestorePointStatus.Text       = S("Backup.RPStatusOk");
            RestorePointResultText.Text   = S("Backup.RPCreated");
            RestorePointResultText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 230, 130));
            RestorePointResult.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(255, 10, 32, 10));
            RestorePointResult.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(26, 107, 26));
            RestorePointResult.Visibility = Visibility.Visible;
            _restorePointCreated          = true;
        }
        else
        {
            BtnCreateRestore.Content      = S("Backup.RPFailed");
            RestorePointStatus.Text       = $"{S("Backup.RPStatusFail")}\n{errorMsg}";
            RestorePointResultText.Text   = S("Backup.RPFailed");
            RestorePointResultText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 79, 79));
            RestorePointResult.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(255, 30, 15, 15));
            RestorePointResult.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(107, 30, 30));
            RestorePointResult.Visibility = Visibility.Visible;
        }

        BtnCreateRestore.IsEnabled = true;
    }

    private (bool success, string error) CreateRestorePoint()
    {
        const string regPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";
        try
        {
            // Bypass the 24h frequency limit Windows enforces by default
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath, writable: true))
                key?.SetValue("SystemRestorePointCreationFrequency", 0, Microsoft.Win32.RegistryValueKind.DWord);

            string desc = $"FPSOptimizer_{DateTime.Now:yyyyMMdd_HHmmss}";
            string script =
                $"Enable-ComputerRestore -Drive 'C:\\' -ErrorAction SilentlyContinue; " +
                $"Checkpoint-Computer -Description '{desc}' -RestorePointType MODIFY_SETTINGS -ErrorAction Stop";

            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{script}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var proc = Process.Start(psi)
                ?? throw new Exception("Process could not start.");
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit(90_000);

            if (proc.ExitCode != 0)
                return (false, stderr.Trim());

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
        finally
        {
            // Restore default frequency (1440 min = 24h)
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath, writable: true);
                key?.SetValue("SystemRestorePointCreationFrequency", 1440, Microsoft.Win32.RegistryValueKind.DWord);
            }
            catch { /* non-critical */ }
        }
    }

    public void ApplyToOptions()
    {
        MainWindow.Options.CreateRestorePoint = ChkRestorePoint.IsChecked == true && !_restorePointCreated;
        MainWindow.Options.CreateRegBackup    = ChkRegBackup.IsChecked    == true;
        MainWindow.Options.CreateSvcBackup    = ChkSvcBackup.IsChecked    == true;
    }
}
