using System.Windows;
using System.Windows.Media;
using FPSOptimizer.Core;
using FPSOptimizer.UI.Pages;
using FPSOptimizer.Localization;
using AppLanguage = FPSOptimizer.Localization.Language;
using System.Linq;

namespace FPSOptimizer.UI;

public partial class MainWindow : Window
{
    private int _currentStep = 0;
    private readonly List<System.Windows.Controls.Page> _pages;
    private readonly List<StepModel> _steps;
    public static SystemInfo? SysInfo { get; private set; }
    public static OptimizationOptions Options { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        // Set language BEFORE creating pages, but disconnect event temporarily
        var savedLanguage = Options.Language;
        StringResources.CurrentLanguage = savedLanguage;

        _pages = new List<System.Windows.Controls.Page>
        {
            new PageWelcome(),   // 0
            new PageScan(),      // 1
            new PageBackup(),    // 2
            new PageProfiles(),  // 3
            new PageOptions(),   // 4
            new PageExecute(),   // 5
            new PageDone()       // 6
        };

        _steps = new List<StepModel>
        {
            new("1", "Bun venit",   "Introducere"),
            new("2", "Scanare",     "Analiza PC"),
            new("3", "Protecție",   "Backup & Restore"),
            new("4", "Profil",      "Preset optimizare"),
            new("5", "Configurare", "Alegeri opțiuni"),
            new("6", "Execuție",    "Optimizare live"),
            new("7", "Finalizat",   "Gata!"),
        };

        StepList.ItemsSource = _steps;
        BuildDots();
        GoToStep(0);

        // Now safe to set index — _pages is ready
        LanguageSelector.SelectionChanged -= LanguageSelector_SelectionChanged;
        LanguageSelector.SelectedIndex = savedLanguage == AppLanguage.EN ? 1 : 0;
        LanguageSelector.SelectionChanged += LanguageSelector_SelectionChanged;

        UpdateUILanguage();

        // Load sys info in background
        Task.Run(async () =>
        {
            SysInfo = await new SystemScanner().ScanAsync();
            Dispatcher.Invoke(() =>
            {
                SysInfoSidebar.Text = $"{SysInfo.Cpu.Name}\n{SysInfo.Gpu.Name}\n{SysInfo.Ram.Gb:0.#} GB RAM";
                if (_pages[1] is PageScan scanPage) scanPage.PopulateInfo(SysInfo);
            });
        });
    }

    private void LanguageSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var language = LanguageSelector.SelectedIndex == 0 ? AppLanguage.RO : AppLanguage.EN;
        StringResources.CurrentLanguage = language;
        Options.Language = language;
        UpdateUILanguage();
    }

    private void UpdateUILanguage()
    {
        AdminLabel.Text = StringResources.Get("SideBar.Admin");

        // Update step labels
        string[] titleKeys    = { "Step.Welcome","Step.Scan","Step.Backup","Step.Profiles","Step.Options","Step.Execute","Step.Done" };
        string[] subtitleKeys = { "Step.WelcomeDesc","Step.ScanDesc","Step.BackupDesc","Step.ProfilesDesc","Step.OptionsDesc","Step.ExecuteDesc","Step.DoneDesc" };
        for (int i = 0; i < _steps.Count && i < titleKeys.Length; i++)
        {
            _steps[i].Title    = StringResources.Get(titleKeys[i]);
            _steps[i].Subtitle = StringResources.Get(subtitleKeys[i]);
        }

        // Refresh binding
        StepList.ItemsSource = null;
        StepList.ItemsSource = _steps;

        // Update buttons
        BtnBack.Content = StringResources.Get("Btn.Back");
        UpdateNextButton();

        // Propagate to all pages
        if (_pages[0] is Pages.PageWelcome  pw) pw.UpdateLanguage();
        if (_pages[1] is Pages.PageScan     ps) ps.UpdateLanguage();
        if (_pages[2] is Pages.PageBackup   pb) pb.UpdateLanguage();
        if (_pages[3] is Pages.PageProfiles pp) pp.UpdateLanguage();
        if (_pages[4] is Pages.PageOptions  po) po.UpdateLanguage();
        if (_pages[5] is Pages.PageExecute  pe) pe.UpdateLanguage();
        if (_pages[6] is Pages.PageDone     pd) pd.UpdateLanguage();
    }

    private void UpdateNextButton()
    {
        if (_currentStep == _pages.Count - 1)
        {
            BtnNext.Content = StringResources.Get("Btn.Close");
            BtnNext.Background = new SolidColorBrush(Color.FromRgb(0, 90, 40));
        }
        else if (_currentStep == 5)
        {
            BtnNext.Content = StringResources.Get("Btn.Results");
        }
        else
        {
            BtnNext.Content = StringResources.Get("Btn.Next");
            BtnNext.Background = new SolidColorBrush(Color.FromRgb(10, 110, 204));
        }
    }

    private void BuildDots()
    {
        var dots = new List<DotModel>();
        for (int i = 0; i < _pages.Count; i++)
            dots.Add(new DotModel(i == _currentStep));
        DotIndicators.ItemsSource = dots;
    }

    public void GoToStep(int index)
    {
        if (index < 0 || index >= _pages.Count) return;
        _currentStep = index;

        MainFrame.Navigate(_pages[index]);
        UpdateStepVisuals();
        BuildDots();

        BtnBack.Visibility = index > 0 ? Visibility.Visible : Visibility.Collapsed;

        UpdateNextButton();
    }

    private void UpdateStepVisuals()
    {
        var accent  = new SolidColorBrush(Color.FromRgb(30, 180, 255));
        var green   = new SolidColorBrush(Color.FromRgb(0, 230, 130));
        var white   = new SolidColorBrush(Color.FromRgb(220, 225, 245));
        var lgray   = new SolidColorBrush(Color.FromRgb(140, 145, 175));
        var gray    = new SolidColorBrush(Color.FromRgb(50, 55, 80));
        var transp  = Brushes.Transparent;
        var accentBg= new SolidColorBrush(Color.FromRgb(10, 30, 55));
        var greenBg = new SolidColorBrush(Color.FromRgb(0, 30, 15));

        for (int i = 0; i < _steps.Count; i++)
        {
            var s = _steps[i];
            if (i < _currentStep)
            {
                s.CircleBg     = greenBg;
                s.CircleBorder = green;
                s.NumberColor  = green;
                s.TitleColor   = green;
                s.ActiveBar    = transp;
            }
            else if (i == _currentStep)
            {
                s.CircleBg     = accentBg;
                s.CircleBorder = accent;
                s.NumberColor  = accent;
                s.TitleColor   = white;
                s.ActiveBar    = accent;
            }
            else
            {
                s.CircleBg     = gray;
                s.CircleBorder = gray;
                s.NumberColor  = lgray;
                s.TitleColor   = lgray;
                s.ActiveBar    = transp;
            }
        }
        // Refresh binding
        StepList.ItemsSource = null;
        StepList.ItemsSource = _steps;
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == _pages.Count - 1) { Close(); return; }

        // Validation hooks per page
        if (_currentStep == 1 && SysInfo == null)
        {
            if (MessageBox.Show(StringResources.Get("Scan.NotDone"), StringResources.Get("Options.Warning"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
        }

        // Sync backup choices when leaving step 2 (Backup)
        if (_currentStep == 2 && _pages[2] is Pages.PageBackup backupPage)
            backupPage.ApplyToOptions();

        // Confirm dialog when leaving step 4 (Options)
        if (_currentStep == 4)
        {
            var selected = Options.SelectedModules.Count;
            if (selected == 0)
            {
                MessageBox.Show(StringResources.Get("Options.MinOne"), StringResources.Get("Options.Warning"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selNames = ModuleDefinitions.All
                .Where(m => Options.SelectedModules.Contains(m.Key))
                .Select(m => $"  • {m.Title}")
                .ToList();
            var msg = string.Format(StringResources.Get("Options.ConfirmMsg"),
                selNames.Count, string.Join("\n", selNames));
            var r = MessageBox.Show(msg, StringResources.Get("Options.ConfirmTitle"),
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;
        }

        GoToStep(_currentStep + 1);

        // Auto-trigger execution when landing on step 5 (Execute)
        if (_currentStep == 5 && _pages[5] is PageExecute execPage)
            execPage.StartOptimization(Options, SysInfo ?? new SystemInfo());
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
        => GoToStep(_currentStep - 1);
}

// ─── ViewModels ───────────────────────────────────────────────────────────────

public class StepModel : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    void OnProp(string n) => PropertyChanged?.Invoke(this, new(n));

    public string Number { get; }
    private string _title = "";
    private string _subtitle = "";

    public string Title
    {
        get => _title;
        set { _title = value; OnProp(nameof(Title)); }
    }

    public string Subtitle
    {
        get => _subtitle;
        set { _subtitle = value; OnProp(nameof(Subtitle)); }
    }

    private System.Windows.Media.Brush _circleBg = System.Windows.Media.Brushes.Gray;
    private System.Windows.Media.Brush _circleBorder = System.Windows.Media.Brushes.Gray;
    private System.Windows.Media.Brush _numberColor = System.Windows.Media.Brushes.Gray;
    private System.Windows.Media.Brush _titleColor  = System.Windows.Media.Brushes.Gray;
    private System.Windows.Media.Brush _activeBar   = System.Windows.Media.Brushes.Transparent;

    public System.Windows.Media.Brush CircleBg     { get => _circleBg;     set { _circleBg=value; OnProp(nameof(CircleBg)); } }
    public System.Windows.Media.Brush CircleBorder { get => _circleBorder; set { _circleBorder=value; OnProp(nameof(CircleBorder)); } }
    public System.Windows.Media.Brush NumberColor  { get => _numberColor;  set { _numberColor=value; OnProp(nameof(NumberColor)); } }
    public System.Windows.Media.Brush TitleColor   { get => _titleColor;   set { _titleColor=value; OnProp(nameof(TitleColor)); } }
    public System.Windows.Media.Brush ActiveBar    { get => _activeBar;    set { _activeBar=value; OnProp(nameof(ActiveBar)); } }

    public StepModel(string num, string title, string sub)
    {
        Number = num;
        _title = title;
        _subtitle = sub;
    }
}

public class DotModel
{
    public double Size  { get; }
    public System.Windows.Media.Brush Color { get; }
    public DotModel(bool active)
    {
        Size  = active ? 10 : 6;
        Color = active
            ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 180, 255))
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 55, 80));
    }
}
