using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace BACApp.UI;

public partial class App : Application
{

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Host.StartHost().Wait();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindowView
            {
                DataContext = Host.GetService<ViewModels.MainWindowViewModel>()
            };

            Program.ActivationHandler = request =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (desktop.MainWindow is not Window window)
                    {
                        return;
                    }

                    window.Show();

                    if (window.WindowState == WindowState.Minimized)
                    {
                        window.WindowState = WindowState.Normal;
                    }

                    if (request == SingleInstanceCoordinator.ActivationRequest.Maximize)
                    {
                        window.WindowState = WindowState.Maximized;
                    }

                    window.Activate();

                    // Often needed on Windows to reliably raise/focus.
                    window.Topmost = true;
                    window.Topmost = false;

                    window.Focus();
                });
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}