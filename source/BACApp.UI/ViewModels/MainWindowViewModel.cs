using BACApp.Core.Messages;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Services;
using BACApp.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace BACApp.UI.ViewModels;

internal partial class MainWindowViewModel : BaseViewModel
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly PageFactory _pageFactory;

    [ObservableProperty]
    private string _windowTitle;

    [ObservableProperty]
    private bool _isPaneOpen = false;

    [ObservableProperty]
    private bool _isLoggedIn = false;

    [ObservableProperty]
    private bool _isAutoLogin = false;

    [ObservableProperty]
    private bool _isLogsEnabled = false;

    [ObservableProperty]
    private bool _isLogsAirframeEnabled = false;

    [ObservableProperty]
    private bool _isReportsEnabled = false;

    [ObservableProperty]
    private PageViewModel _currentPage;



    public MainWindowViewModel()
    {
        _logger = null;
        _authService = null;

        WindowTitle = "Cloudbase App";
    }


    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, 
        IAuthService authService,
        PageFactory pageFactory)
    {
        _logger = logger;
        _authService = authService;
        _pageFactory = pageFactory;

        var informationVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        WindowTitle = $"Cloudbase App [{informationVersion}]";

        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (r, m) =>
        {
            CurrentPage = _pageFactory.GetPageViewModel<CalendarPageViewModel>();
            IsLoggedIn = true;

            if (!IsAutoLogin)
            {
                IsLogsEnabled = true;
                IsLogsAirframeEnabled = true;
                IsReportsEnabled = true;
            }

            WindowTitle = $"Cloudbase App [{informationVersion}] : {_authService.UserCompany.CompanyName}";
        });

        WeakReferenceMessenger.Default.Register<AutoLoginMessage>(this, (r, m) =>
        {
            IsAutoLogin = true;
            IsLogsEnabled = false;
            IsLogsAirframeEnabled = false;
            IsReportsEnabled = false;
        });

        CurrentPage = _pageFactory.GetPageViewModel<LoginPageViewModel>();

    }

    partial void OnCurrentPageChanged(PageViewModel oldValue, PageViewModel newValue)
    {
        if (oldValue is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [RelayCommand]
    private void GoToLogin() => CurrentPage = _pageFactory.GetPageViewModel<LoginPageViewModel>();

    [RelayCommand]
    private void GoToCalendar() => CurrentPage = _pageFactory.GetPageViewModel<CalendarPageViewModel>();

    [RelayCommand]
    private void GoToReports() => CurrentPage = _pageFactory.GetPageViewModel<ReportsPageViewModel>();

    [RelayCommand]
    private void GoToLogs() => CurrentPage = _pageFactory.GetPageViewModel<LogsPageViewModel>();

    [RelayCommand]
    private void GoToLogsAirframe() => CurrentPage = _pageFactory.GetPageViewModel<LogsAirframePageViewModel>();

}
