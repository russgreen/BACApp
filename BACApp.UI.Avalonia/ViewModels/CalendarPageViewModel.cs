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
        IAircraftService aircraftService) 
        : base(ApplicationPageNames.Calendar)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;

        // Defer async work; do not block constructor
        _ = LoadAsync();

        Resources.Add(new Resource { Id = 1, Title = "Aircraft A", Comment = "[8.5 hrs]" });
        Resources.Add(new Resource { Id = 2, Title = "Aircraft B", Comment = "[33.5 hrs]" });
        Resources.Add(new Resource { Id = 3, Title = "Aircraft C", Comment = "[50 hrs]" });


        var day = new DateTimeOffset(DateTime.Today, TimeSpan.Zero);
        Events.Add(new Event
        {
            ResourceId = 1,
            Start = day.AddHours(8).ToString(),
            End = day.AddHours(10).ToString(),
            Title = "Member Name"
        });
        Events.Add(new Event
        {
            ResourceId = 2,
            Start = day.AddHours(9).ToString(),
            End = day.AddHours(12).ToString(),
            Title = "Member Name"
        });
        Events.Add(new Event
        {
            ResourceId = 3,
            Start = day.AddHours(5).ToString(),
            End = day.AddHours(22).ToString(),
            Title = "Maintenance"
        });
        Events.Add(new Event
        {
            ResourceId = 1,
            Start = day.AddHours(12).ToString(),
            End = day.AddHours(14).ToString(),
            Title = "Member Name"
        });

    }

    private async Task LoadAsync(CancellationToken ct = default)
    {

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
    }

}
