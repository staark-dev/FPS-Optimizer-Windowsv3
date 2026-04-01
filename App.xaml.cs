using System.Windows;

namespace FPSOptimizer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Global exception handler
        DispatcherUnhandledException += (s, ex) =>
        {
            var inner = ex.Exception.InnerException ?? ex.Exception;
            var msg   = $"{ex.Exception.Message}\n\n{inner.GetType().Name}:\n{inner.Message}\n\n{inner.StackTrace?.Split('\n').FirstOrDefault()}";
            MessageBox.Show(msg, "FPS Optimizer - Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };
    }
}
