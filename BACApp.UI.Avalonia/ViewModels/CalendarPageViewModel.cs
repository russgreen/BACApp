using Avalonia.Media;
using BACApp.Core.Extensions;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Avalonia.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class CalendarPageViewModel : PageViewModel, IDisposable
{
    private readonly ILogger<LogsPageViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IAircraftService _aircraftService;
    private readonly ICalendarService _calendarService;
    private readonly ITechlogService _techlogService;

    private readonly CancellationTokenSource _refreshCts = new();
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    [ObservableProperty]
    private ObservableCollection<BookingResource> _resources = new();

    [ObservableProperty]
    private ObservableCollection<BookingEvent> _events = new();

    [ObservableProperty]
    private string _messageText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDay))]
    private DateTime _selectedDate = DateTime.Today;

    public DateOnly SelectedDay => DateOnly.FromDateTime(SelectedDate);

    public CalendarPageViewModel(ILogger<LogsPageViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        ICalendarService calendarService,
        ITechlogService techlogService) 
        : base(ApplicationPageNames.Calendar)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _calendarService = calendarService;
        _techlogService = techlogService;

        // Defer async work; do not block constructor
        _ = LoadAsync(_refreshCts.Token);

        // Background refresh: first run 1 hour after creation, then hourly within business hours.
        _ = RunHourlyRefreshLoopAsync(_refreshCts.Token);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        await _loadGate.WaitAsync(ct);
        try
        {
            var resources = await _calendarService.GetResourcesAsync(SelectedDay, ct);
            Resources = new ObservableCollection<BookingResource>(resources);
            
            var events = await GetEventsFromTodayToSelectedDayAsync(SelectedDay, ct);
            Events = new ObservableCollection<BookingEvent>(events);

           _ = SetMaintenanceHoursAsync(ct);

            SetMessageText();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load calendar data for {day}.", SelectedDay);
        }
        finally
        {
            _loadGate.Release();
        }
    }

    private async Task RunHourlyRefreshLoopAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(TimeSpan.FromHours(1), ct);

            while (!ct.IsCancellationRequested)
            {
                var now = DateTime.Now;

                EnsureSelectedDateTracksToday(now);

                if (IsWithinBusinessHours(now))
                {
                    await LoadAsync(ct);
                }

                await Task.Delay(TimeSpan.FromHours(1), ct);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown/navigation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Calendar refresh loop failed.");
        }
    }

    private static bool IsWithinBusinessHours(DateTime localNow)
    => localNow.TimeOfDay >= TimeSpan.FromHours(8)
        && localNow.TimeOfDay < TimeSpan.FromHours(18);

    private void EnsureSelectedDateTracksToday(DateTime localNow)
    {
        // If app runs overnight, keep the calendar on "today".
        if (SelectedDate.Date != localNow.Date)
        {
            SelectedDate = localNow;
        }
    }

    private async Task<IReadOnlyList<BookingEvent>> GetEventsFromTodayToSelectedDayAsync(DateOnly selectedDay, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (selectedDay <= today)
        {
            return await _calendarService.GetEventsAsync(selectedDay, ct);
        }

        var totalDays = selectedDay.DayNumber - today.DayNumber + 1;
        var dayList = Enumerable.Range(0, totalDays).Select(i => today.AddDays(i)).ToArray();

        // Modest concurrency: balance latency vs API load
        const int maxConcurrency = 6;
        using var throttler = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var tasks = dayList.Select(async day =>
        {
            await throttler.WaitAsync(ct);
            try
            {
                var dayEvents = await _calendarService.GetEventsAsync(day, ct);
                return dayEvents;
            }
            finally
            {
                throttler.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(x => x).ToArray();
    }


    private async Task SetMaintenanceHoursAsync(CancellationToken ct = default)
    {
        if (SelectedDay < DateOnly.FromDateTime(DateTime.Today))
        {
            // don't bother with past dates
            return;
        }

        // Snapshot to avoid collection changing while awaits are in flight
        var day = SelectedDay;

        var resources = Resources.Where(x => x.Id < 100).ToArray();
        var allEvents = Events.ToArray();

        const int maxConcurrency = 6;
        using var throttler = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var tasks = resources.Select(async resource =>
        {
            await throttler.WaitAsync(ct);
            try
            {
                var title = resource.Title ?? string.Empty;
                var idx = title.IndexOf(" (", StringComparison.Ordinal);
                var registration = (idx >= 0 ? title[..idx] : title).Trim();

                if (string.IsNullOrEmpty(registration))
                {
                    return;
                }

                var resourceId = resource.Id;

                _logger.LogDebug("Fetching maintenance data for {registration}", registration);

                var maintenanceData = await _techlogService.GetMaintenanceDataAsync(registration, day, ct);
                if (maintenanceData == null)
                {
                    return;
                }

                if (SelectedDay != day)
                {
                    return;
                }

                var baseRemaining = ParseHoursMinutesOrZero(maintenanceData.TotalNextCheckHours);

                // Keep existing resource comment behavior (top-level)
                resource.Comment = $"Next check in {FormatTotalHoursMinutes(baseRemaining)}";

                // Apply BookingEvent.Comment per requirements
                ApplyRemainingHoursToBookingEvents(resourceId, baseRemaining, allEvents);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Invalid maintenance hours format for {resourceTitle}", resource.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set maintenance hours/comments for {resourceTitle}", resource.Title);
            }
            finally
            {
                throttler.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private void ApplyRemainingHoursToBookingEvents(int resourceId, TimeSpan totalNextCheckHours, BookingEvent[] allEvents)
    {
        var now = DateTimeOffset.Now;

        var aircraftEvents = allEvents
            .Where(e => e.ResourceId == resourceId)
            .Where(e => !e.Title.Contains("maintenance", StringComparison.InvariantCultureIgnoreCase))
            .Where(e => string.IsNullOrWhiteSpace(e.Rendering))
            .OrderBy(e => e.EndTime)
            .ToArray();

        var remaining = totalNextCheckHours;

        foreach (var ev in aircraftEvents)
        {
            // Requirement: for events when date is today and EndTime has passed,
            // use maintenanceData.TotalNextCheckHours (raw)
            if (ev.EndTime <= now)
            {
                ev.Comment = FormatTotalHoursMinutes(totalNextCheckHours);
                continue;
            }

            // For all other events where EndTime has not passed, subtract estimated flight time cumulatively.
            var flightTime = ev.GetFlightTime() ?? TimeSpan.Zero;
            remaining -= flightTime;
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            ev.Comment = FormatTotalHoursMinutes(remaining);
        }
    }

    private static TimeSpan ParseHoursMinutesOrZero(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TimeSpan.Zero;
        }

        var s = value.Trim();
        var parts = s.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length is < 2 or > 3)
        {
            return TimeSpan.Zero;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours) || hours < 0)
        {
            return TimeSpan.Zero;
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes) || minutes is < 0 or > 59)
        {
            return TimeSpan.Zero;
        }

        var seconds = 0;
        if (parts.Length == 3
            && (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds) || seconds is < 0 or > 59))
        {
            return TimeSpan.Zero;
        }

        return new TimeSpan(hours, minutes, seconds);
    }

    private static string FormatTotalHoursMinutes(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            value = TimeSpan.Zero;
        }

        var totalHours = (int)value.TotalHours;
        return string.Create(CultureInfo.InvariantCulture, $"{totalHours:00}:{value.Minutes:00}");
    }

    private void SetMessageText()
    {
        MessageText = $"Calendar last updated at {DateTime.Now:dd/MM/yyy H:mm}";
    }

    [RelayCommand]
    private void ResourceClick(BookingResource resource)
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
    private void EventClick(BookingEvent resourceEvent)
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

        _ = LoadAsync(_refreshCts.Token);
    }

    public void Dispose()
    {
        _refreshCts.Cancel();
        _refreshCts.Dispose();
        _loadGate.Dispose();

        GC.SuppressFinalize(this);
    }
}
