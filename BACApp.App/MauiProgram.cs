using Microsoft.Extensions.Logging;
using BACApp.Core.Abstractions;
using BACApp.Core.Services;
using System.Net.Http;

namespace BACApp.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// API and services
		var apiBase = new Uri("https://v1.cbga-api.com");
#if DEBUG
        apiBase = new Uri("https://testing.cbga-api.com");
#endif

        builder.Services.AddSingleton<ITokenStore, MauiTokenStore>();
		builder.Services.AddTransient<AuthHeaderHandler>();
		builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
		{
			client.BaseAddress = apiBase;
			client.Timeout = TimeSpan.FromSeconds(30);
		})
		.AddHttpMessageHandler<AuthHeaderHandler>();

		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<ICalendarService, CalendarService>();
		builder.Services.AddSingleton<IReportsService, ReportsService>();

		// Pages & ViewModels
		builder.Services.AddTransient<ViewModels.LoginViewModel>();
		builder.Services.AddTransient<Pages.LoginPage>();
		builder.Services.AddTransient<ViewModels.CalendarViewModel>();
		builder.Services.AddTransient<Pages.CalendarPage>();
		builder.Services.AddTransient<ViewModels.FlightLogsViewModel>();
		builder.Services.AddTransient<Pages.FlightLogsPage>();
		builder.Services.AddTransient<ViewModels.TechLogsViewModel>();
		builder.Services.AddTransient<Pages.TechLogsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
