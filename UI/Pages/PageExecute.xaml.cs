using FPSOptimizer.Core;
using FPSOptimizer.Localization;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FPSOptimizer.UI.Pages;

public partial class PageExecute : Page
{
    private readonly Stopwatch _sw = new();
    private System.Windows.Threading.DispatcherTimer? _timer;

    public PageExecute() => InitializeComponent();

    public void UpdateLanguage()
    {
        TxtTitle.Text   = StringResources.Get("Execute.Title");
        TxtDesc.Text    = StringResources.Get("Execute.Desc");
        StatusText.Text = StringResources.Get("Execute.Ready");
    }

    public void StartOptimization(OptimizationOptions opts, SystemInfo si)
    {
        _sw.Restart();
        _timer = new System.Windows.Threading.DispatcherTimer
            { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) =>
            TimeLabel.Text = string.Format(StringResources.Get("Execute.Elapsed"), $"{_sw.Elapsed:mm\\:ss}");
        _timer.Start();

        // Build chips for each selected module
        ChipsPanel.Children.Clear();
        foreach (var mod in ModuleDefinitions.All.Where(m => opts.SelectedModules.Contains(m.Key)))
            ChipsPanel.Children.Add(MakeChip(mod.Key, mod.Icon, mod.Title));

        var progress = new Progress<int>(pct =>
        {
            MainProgress.Value = pct;
            PctLabel.Text = $"{pct}%";
        });

        var engine = new OptimizerEngine(AppendLog, progress);

        Task.Run(async () =>
        {
            await engine.RunAsync(opts, si);
            Dispatcher.Invoke(() =>
            {
                _timer.Stop();
                StatusIcon.Text = "✅";
                StatusText.Text = StringResources.Get("Done.Title");
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 130));
                LiveDot.Background    = new SolidColorBrush(Color.FromRgb(0, 230, 130));
                MainProgress.Value    = 100;
                PctLabel.Text         = "100%";
                TimeLabel.Text        = string.Format(StringResources.Get("Execute.Elapsed"), $"{_sw.Elapsed:mm\\:ss}");
            });
        });
    }

    // ── COLORED LOG ─────────────────────────────────────────────
    private void AppendLog(string message, LogType type)
    {
        Dispatcher.Invoke(() =>
        {
            var doc = LogBox.Document;
            var para = new Paragraph { Margin = new Thickness(0) };

            var (fg, prefix) = type switch
            {
                LogType.Ok    => (Color.FromRgb(0,   230, 130), "✓ "),
                LogType.Warn  => (Color.FromRgb(255, 200,  50), "! "),
                LogType.Error => (Color.FromRgb(255,  79,  79), "✗ "),
                LogType.Head  => (Color.FromRgb( 30, 180, 255), "► "),
                LogType.Skip  => (Color.FromRgb( 90,  95, 130), "○ "),
                _             => (Color.FromRgb(180, 185, 210), "  "),
            };

            // Timestamp in gray
            var ts = new Run($"{DateTime.Now:HH:mm:ss} ")
                { Foreground = new SolidColorBrush(Color.FromRgb(60, 65, 95)) };
            var body = new Run(prefix + message)
                { Foreground = new SolidColorBrush(fg) };

            para.Inlines.Add(ts);
            para.Inlines.Add(body);
            doc.Blocks.Add(para);

            LogScroll.ScrollToEnd();

            // Update status bar
            if (type == LogType.Head || type == LogType.Ok)
                StatusText.Text = message;

            // Update chip if it's a module start
            UpdateChipActive(message);
        });
    }

    // ── CHIPS ────────────────────────────────────────────────────
    private readonly Dictionary<string, Border> _chips = new();

    private Border MakeChip(string key, string icon, string title)
    {
        var chip = new Border
        {
            Background    = new SolidColorBrush(Color.FromRgb(18, 20, 38)),
            BorderBrush   = new SolidColorBrush(Color.FromRgb(35, 40, 75)),
            BorderThickness = new Thickness(1),
            CornerRadius  = new CornerRadius(20),
            Padding       = new Thickness(10, 4, 10, 4),
            Margin        = new Thickness(0, 0, 6, 6),
        };
        var sp = new StackPanel { Orientation = Orientation.Horizontal };
        sp.Children.Add(new TextBlock
            { Text = icon, FontSize = 12, Margin = new Thickness(0, 0, 4, 0),
              VerticalAlignment = VerticalAlignment.Center });
        sp.Children.Add(new TextBlock
            { Text = title.Length > 22 ? title[..22] + "…" : title,
              FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(140, 145, 175)),
              VerticalAlignment = VerticalAlignment.Center });
        chip.Child = sp;
        _chips[key] = chip;
        return chip;
    }

    private void UpdateChipActive(string msg)
    {
        foreach (var mod in ModuleDefinitions.All)
        {
            if (!_chips.TryGetValue(mod.Key, out var chip)) continue;
            if (msg.Contains(mod.Title, StringComparison.OrdinalIgnoreCase))
            {
                chip.Background   = new SolidColorBrush(Color.FromRgb(10, 30, 55));
                chip.BorderBrush  = new SolidColorBrush(Color.FromRgb(30, 180, 255));
                var tb = ((StackPanel)chip.Child).Children.OfType<TextBlock>().Last();
                tb.Foreground = new SolidColorBrush(Color.FromRgb(30, 180, 255));
            }
            else if (msg.StartsWith("✓") || msg.StartsWith("─"))
            {
                // Mark done chips green
                chip.Background  = new SolidColorBrush(Color.FromRgb(0, 20, 10));
                chip.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 150, 80));
            }
        }
    }
}
