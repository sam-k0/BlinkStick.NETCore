using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace BlinkStickLinux;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // Safety net: a GUI action (a failed config save, a missing device, etc.)
        // should never take the whole app down. Log it and keep running; the
        // individual handlers surface a friendly message in the window.
        Dispatcher.UIThread.UnhandledException += (s, e) =>
        {
            Console.Error.WriteLine("Unhandled exception: " + e.Exception);
            e.Handled = true;
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
