using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BACApp.Core.Services;
using System;

namespace BACApp.UI.Avalonia;

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
            desktop.MainWindow = new Views.MainWindow
            {
                DataContext = Host.GetService<ViewModels.MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}