using BACApp.UI.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class MainWindowViewModel : BaseViewModel
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly PageFactory _pageFactory;

    [ObservableProperty]
    private bool _isPaneOpen = true;

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

    }

    [RelayCommand]
    private void GoToLogin() => CurrentPage = _pageFactory.GetPageViewModel<LoginPageViewModel>();

    [RelayCommand]
    private void GoToCalendar() => CurrentPage = _pageFactory.GetPageViewModel<CalendarPageViewModel>();

    [RelayCommand]
    private void GoToReports() => CurrentPage = _pageFactory.GetPageViewModel<ReportsPageViewModel>();

    [RelayCommand]
    private void GoToLogs() => CurrentPage = _pageFactory.GetPageViewModel<LogsPageViewModel>();


}
