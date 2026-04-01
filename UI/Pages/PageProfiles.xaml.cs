using FPSOptimizer.Core;
using FPSOptimizer.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FPSOptimizer.UI.Pages;

public partial class PageProfiles : Page
{
    // Module sets per profile
    private static readonly HashSet<string> SafeModules    = new() { "PowerPlan", "CPU", "RAM", "GPU" };
    private static readonly HashSet<string> BalancedModules = new() { "PowerPlan", "CPU", "RAM", "Storage", "GPU", "RegistryFPS", "Network", "Telemetry" };
    private static readonly HashSet<string> GamingModules   = new(ModuleDefinitions.All.Select(m => m.Key));

    private string _selectedProfile = "Balanced";

    public PageProfiles()
    {
        InitializeComponent();
        PopulateModuleLists();
        ApplySelection("Balanced");
        UpdateLanguage();
    }

    public void UpdateLanguage()
    {
        var S = StringResources.Get;
        TxtTitle.Text             = S("Profiles.Title");
        TxtDesc.Text              = S("Profiles.Desc");
        TxtSafeTitle.Text         = S("Profiles.Safe");
        TxtSafeSubtitle.Text      = S("Profiles.SafeDesc");
        TxtBalancedTitle.Text     = S("Profiles.Balanced");
        TxtBalancedSubtitle.Text  = S("Profiles.BalancedDesc");
        TxtGamingTitle.Text       = S("Profiles.Gaming");
        TxtGamingSubtitle.Text    = S("Profiles.GamingDesc");
        TxtSafeBadge.Text         = string.Format(S("Profiles.ModuleCount"), SafeModules.Count);
        TxtBalancedBadge.Text     = string.Format(S("Profiles.ModuleCount"), BalancedModules.Count);
        TxtGamingBadge.Text       = string.Format(S("Profiles.ModuleCount"), GamingModules.Count);
    }

    private void PopulateModuleLists()
    {
        FillPanel(PanelSafeModules,    SafeModules);
        FillPanel(PanelBalancedModules, BalancedModules);
        FillPanel(PanelGamingModules,   GamingModules);
    }

    private static void FillPanel(StackPanel panel, HashSet<string> keys)
    {
        panel.Children.Clear();
        foreach (var mod in ModuleDefinitions.All.Where(m => keys.Contains(m.Key)))
        {
            panel.Children.Add(new TextBlock
            {
                Text       = $"{mod.Icon}  {mod.Title}",
                FontSize   = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 145, 175)),
                Margin     = new Thickness(0, 2, 0, 2),
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private void CardSafe_Click(object sender, MouseButtonEventArgs e)    => ApplySelection("Safe");
    private void CardBalanced_Click(object sender, MouseButtonEventArgs e) => ApplySelection("Balanced");
    private void CardGaming_Click(object sender, MouseButtonEventArgs e)   => ApplySelection("Gaming");

    private void ApplySelection(string profile)
    {
        _selectedProfile = profile;

        var accentColor  = Color.FromRgb(30, 180, 255);
        var greenColor   = Color.FromRgb(0, 200, 100);
        var orangeColor  = Color.FromRgb(255, 140, 30);

        // Reset all cards
        SetCardSelected(CardSafe,     false, greenColor);
        SetCardSelected(CardBalanced, false, accentColor);
        SetCardSelected(CardGaming,   false, orangeColor);

        // Highlight selected
        switch (profile)
        {
            case "Safe":
                SetCardSelected(CardSafe, true, greenColor);
                MainWindow.Options.SelectedModules = new HashSet<string>(SafeModules);
                break;
            case "Balanced":
                SetCardSelected(CardBalanced, true, accentColor);
                MainWindow.Options.SelectedModules = new HashSet<string>(BalancedModules);
                break;
            case "Gaming":
                SetCardSelected(CardGaming, true, orangeColor);
                MainWindow.Options.SelectedModules = new HashSet<string>(GamingModules);
                break;
        }
    }

    private static void SetCardSelected(Border card, bool selected, Color accent)
    {
        if (selected)
        {
            card.BorderBrush = new SolidColorBrush(accent);
            card.Background  = new SolidColorBrush(Color.FromArgb(40,
                accent.R, accent.G, accent.B));
        }
        else
        {
            card.BorderBrush = new SolidColorBrush(Color.FromRgb(35, 40, 80));
            card.Background  = new SolidColorBrush(Color.FromRgb(13, 15, 30));
        }
    }

    public string SelectedProfile => _selectedProfile;
}
