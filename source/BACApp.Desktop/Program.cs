using Avalonia;
using BACApp.UI;
using System;
using System.Linq;
using System.Threading;

namespace BACApp.Desktop;

internal class Program
{
    private static SingleInstanceCoordinator? _singleInstance;
    private static readonly CancellationTokenSource _singleInstanceCts = new();

    [STAThread]
    public static int Main(string[] args)
    {
        const string appId = "BACApp.UI";

        _singleInstance = new SingleInstanceCoordinator(appId);

        if (!_singleInstance.IsPrimaryInstance)
        {
            var request = ParseActivationRequest(args);
            return _singleInstance.SendActivationAsync(request, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        _ = _singleInstance.RunServerAsync(request =>
        {
            App.ActivationHandler?.Invoke(request == SingleInstanceCoordinator.ActivationRequest.Maximize);
            return System.Threading.Tasks.Task.CompletedTask;
        }, _singleInstanceCts.Token);

        var exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        _singleInstanceCts.Cancel();
        _singleInstance.Dispose();

        return exitCode;
    }

    private static SingleInstanceCoordinator.ActivationRequest ParseActivationRequest(string[] args)
    {
        if (args.Any(a => string.Equals(a, "--maximize", StringComparison.OrdinalIgnoreCase)))
        {
            return SingleInstanceCoordinator.ActivationRequest.Maximize;
        }

        return SingleInstanceCoordinator.ActivationRequest.Activate;
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
#if DEBUG
            .LogToTrace()
#endif
        ;
}
