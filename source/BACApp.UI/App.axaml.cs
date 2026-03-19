using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace BACApp.UI;

public partial class App : Application
{
    /// <summary>
    /// Set by the desktop entry point to handle a second-instance activation request.
    /// The boolean argument is <c>true</c> when the window should be maximised.
    /// </summary>
    public static Action<bool>? ActivationHandler { get; set; }

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

            ActivationHandler = maximize =>
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

                    if (maximize)
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
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new Views.MainView
            {
                DataContext = Host.GetService<ViewModels.MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}