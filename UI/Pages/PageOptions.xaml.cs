using FPSOptimizer.Core;
using FPSOptimizer.Localization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FPSOptimizer.UI.Pages;

public partial class PageOptions : Page
{
    private readonly List<ModuleViewModel> _vms;

    public PageOptions()
    {
        InitializeComponent();

        string? lastCat = null;
        _vms = ModuleDefinitions.All.Select(m =>
        {
            bool showHeader = m.Category != lastCat;
            lastCat = m.Category;
            var vm = new ModuleViewModel(m, showHeader);
            vm.PropertyChanged += (_, _) => UpdateCount();
            return vm;
        }).ToList();

        ModuleList.ItemsSource = _vms;
        UpdateCount();
    }

    public void UpdateLanguage()
    {
        var S = StringResources.Get;
        TxtTitle.Text         = S("Options.Title");
        TxtDesc.Text          = S("Options.Desc");
        BtnSelectAll.Content  = S("Options.SelectAll");
        BtnSelectNone.Content = S("Options.SelectNone");
        UpdateCount();
    }

    private void UpdateCount()
    {
        int sel = _vms.Count(v => v.IsSelected);
        CountLabel.Text = string.Format(StringResources.Get("Options.Count"), sel, _vms.Count);

        MainWindow.Options.SelectedModules =
            new HashSet<string>(_vms.Where(v => v.IsSelected).Select(v => v.Key));
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        _vms.ForEach(v => v.IsSelected = true);
        UpdateCount();
    }

    private void SelectNone_Click(object sender, RoutedEventArgs e)
    {
        _vms.ForEach(v => v.IsSelected = false);
        UpdateCount();
    }
}

public class ModuleViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Notify(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

    private bool _isSelected;

    public string Key      { get; }
    public string Category { get; }
    public string Icon     { get; }
    public string Title    { get; }
    public string Desc     { get; }
    public string Details  { get; }
    public string RiskLabel{ get; }
    public Brush  RiskColor{ get; }
    public Brush  RiskBadgeBg { get; }
    public Visibility CategoryHeaderVisibility { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; Notify(nameof(IsSelected)); }
    }

    public ModuleViewModel(OptimizationModule m, bool showCatHeader)
    {
        Key        = m.Key;
        Category   = m.Category;
        Icon       = m.Icon;
        Title      = m.Title;
        Desc       = m.Desc;
        Details    = m.Details;
        RiskLabel  = m.RiskLabel;
        _isSelected = m.Default;
        CategoryHeaderVisibility = showCatHeader ? Visibility.Visible : Visibility.Collapsed;

        (RiskColor, RiskBadgeBg) = m.Risk switch
        {
            RiskLevel.Safe     => (Brush(0, 200, 100),  Brush(0, 30, 10)),
            RiskLevel.Cosmetic => (Brush(255, 200, 50),  Brush(40, 30, 0)),
            RiskLevel.Medium   => (Brush(255, 140, 30),  Brush(40, 20, 0)),
            _                  => (Brushes.Gray, Brushes.Transparent)
        };
    }

    private static SolidColorBrush Brush(byte r, byte g, byte b)
        => new(Color.FromRgb(r, g, b));
}
