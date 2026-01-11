using BACApp.Core.Abstractions;
using BACApp.Core.Services;
using BACApp.UI.ViewModels;
using BACApp.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.Threading.Tasks;

namespace BACApp.UI;

internal static class Host
{
    private static IHost _host;

    public static async Task StartHost()
    {
        var apiBase = new Uri("https://v1.cbga-api.com");

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
                services.AddTransient<AuthHeaderHandler>();
                services.AddHttpClient<IApiClient, ApiClient>(client =>
                {
                    client.BaseAddress = apiBase;
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler<AuthHeaderHandler>();

                services.AddSingleton<ITokenStore, TokenStore>();    
                services.AddSingleton<IAuthService, AuthService>();

                services.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
                {
                    _ when type == typeof(LoginPageViewModel) => x.GetRequiredService<LoginPageViewModel>(),
                    _ when type == typeof(CalendarPageViewModel) => x.GetRequiredService<CalendarPageViewModel>(),
                    _ when type == typeof(LogsPageViewModel) => x.GetRequiredService<LogsPageViewModel>(),
                    _ when type == typeof(LogsAirframePageViewModel) => x.GetRequiredService<LogsAirframePageViewModel>(),
                    _ when type == typeof(ReportsPageViewModel) => x.GetRequiredService<ReportsPageViewModel>(),
                    _ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
                });

                services.AddSingleton<PageFactory>();

                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<IAircraftService, AircraftService>();
                services.AddSingleton<ICalendarService, CalendarService>();

                services.AddTransient<ITechlogService, TechlogService>();
                services.AddTransient<IFlightLogsService, FlightLogsService>();
                services.AddTransient<ICsvExportService, CsvExportService>();

                services.AddTransient<LoginPageViewModel>();

                services.AddTransient<CalendarPageViewModel>();
                services.AddTransient<LogsPageViewModel>();

                services.AddTransient<LogsAirframePageViewModel>();
                services.AddTransient<ReportsPageViewModel>();
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
