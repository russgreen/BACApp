using BACApp.Core.Messages;
using BACApp.Core.Services;
using BACApp.UI.Services;
using BACApp.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;

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


        WindowTitle = $"Cloudbase App";

        CurrentPage = _pageFactory.GetPageViewModel<LoginPageViewModel>();

        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (r, m) =>
        {
            CurrentPage = _pageFactory.GetPageViewModel<CalendarPageViewModel>();
            IsLoggedIn = true;
            WindowTitle = $"Cloudbase App : {_authService.UserCompany.CompanyName}";
        });

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
    private void GoToTechLogs() => CurrentPage = _pageFactory.GetPageViewModel<TechLogsPageViewModel>();

}
