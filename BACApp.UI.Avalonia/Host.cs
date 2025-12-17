using BACApp.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Threading.Tasks;

namespace BACApp.UI.Avalonia;

internal static class Host
{
    private static IHost _host;

    public static async Task StartHost()
    {
        //TODO how to set cross platform log storage paths
        var logPath = "log.json"; //System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BACApp", "Log.json");

        //#if DEBUG
        //        logPath = "log.json";
        //#endif

        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

            // Local file - exclude usage tracking logs
            .WriteTo.Logger(l => l
                .WriteTo.File(new JsonFormatter(), logPath,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7));


        Log.Logger = loggerConfig.CreateLogger();

        _host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<IAuthService, AuthService>();

                services.AddSingleton<ViewModels.MainWindowViewModel>();
            })
            .Build();

        await _host.StartAsync();
    }

    public static async Task StartHost(IHost host)
    {
        _host = host;
        await host.StartAsync();
    }

    public static async Task StopHost()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    public static T GetService<T>() where T : class
    {
        return _host.Services.GetRequiredService<T>();
    }
}
