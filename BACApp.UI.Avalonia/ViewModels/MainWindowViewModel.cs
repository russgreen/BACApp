using BACApp.UI.Avalonia.Messages;
using BACApp.UI.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class MainWindowViewModel : BaseViewModel
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly PageFactory _pageFactory;

    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    private bool _isLoggedIn = false;

    [ObservableProperty]
    private PageViewModel _currentPage;

    public string WindowTitle { get; private set; }

    public MainWindowViewModel()
    {
        _logger = null;

        WindowTitle = "BAC Cloudbase App";
    }


    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, PageFactory pageFactory)
    {
        _logger = logger;
        _pageFactory = pageFactory;

        this.WindowTitle = "BAC Cloudbase App";

        _logger.LogDebug(WindowTitle);

        CurrentPage = _pageFactory.GetPageViewModel<LoginPageViewModel>();

        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (r, m) =>
        {
            CurrentPage = _pageFactory.GetPageViewModel<CalendarPageViewModel>();
            IsLoggedIn = true;
        });

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
