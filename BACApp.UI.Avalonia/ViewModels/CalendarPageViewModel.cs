using Avalonia.Media;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Avalonia.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class CalendarPageViewModel : PageViewModel
{
    private readonly ILogger<LogsPageViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IAircraftService _aircraftService;
    private readonly ICalendarService _calendarService;

    [ObservableProperty]
    private ObservableCollection<Resource> _resources = new();

    [ObservableProperty]
    private ObservableCollection<Event> _events = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDay))]
    private DateTime _selectedDate = DateTime.Today;

    public DateOnly SelectedDay => DateOnly.FromDateTime(SelectedDate);

    public CalendarPageViewModel(ILogger<LogsPageViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        ICalendarService calendarService) 
        : base(ApplicationPageNames.Calendar)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _calendarService = calendarService;

        // Defer async work; do not block constructor
        _ = LoadAsync();

    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
         var resources = await _calendarService.GetResourcesAsync(SelectedDay, ct);
         Resources = new ObservableCollection<Resource>(resources);

         var events = await _calendarService.GetEventsAsync(SelectedDay, ct);
         Events = new ObservableCollection<Event>(events);
    }

    [RelayCommand]
    private void ResourceClick(Resource resource)
    {
        if (resource == null)
        {
            return;
        }

        // Handle resource click
        _logger.LogDebug("Resource clicked {resource}", resource);
        // e.g., navigate to resource details, show dialog, etc.
    }

    [RelayCommand]
    private void EventClick(Event resourceEvent)
    {
        if (resourceEvent == null)
        {
            return;
        }

        // Handle event click
        _logger.LogDebug("Event clicked: {resourceEvent}", resourceEvent);
        // e.g., show event details, edit event, etc.
    }

    partial void OnSelectedDateChanged(DateTime oldValue, DateTime newValue)
    {
        _logger.LogDebug("Date changed {date}", newValue);

        _ = LoadAsync();
    }

}
