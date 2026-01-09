using Avalonia;
using System;
using System.Linq;
using System.Threading;

namespace BACApp.UI;

internal class Program
{
    private static SingleInstanceCoordinator? _singleInstance;
    private static readonly CancellationTokenSource SingleInstanceCts = new();

    public static Action<SingleInstanceCoordinator.ActivationRequest>? ActivationHandler { get; internal set; }

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
            ActivationHandler?.Invoke(request);
            return System.Threading.Tasks.Task.CompletedTask;
        }, SingleInstanceCts.Token);

        var exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        SingleInstanceCts.Cancel();
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
