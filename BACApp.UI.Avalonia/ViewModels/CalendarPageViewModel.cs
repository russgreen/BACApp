using Avalonia.Media;
using BACApp.Core.Extensions;
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
            
            var events = await _calendarService.GetEventsAsync(SelectedDay, ct);
            Events = new ObservableCollection<BookingEvent>(events);

           _ = SetMaintenanceHours(ct);

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

    private async Task SetMaintenanceHours(CancellationToken ct = default)
    {
        if (SelectedDay < DateOnly.FromDateTime(DateTime.Today))
        {
            // don't bother with past dates
            return;
        }

        // Snapshot to avoid collection changing while awaits are in flight
        var day = SelectedDay;
        var resources = Resources.Where(x => x.Id < 100).ToArray();

        // Tune as needed; keeps API/UI responsive
        const int maxConcurrency = 6;
        using var throttler = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var tasks = resources.Select(async resource =>
        {
            await throttler.WaitAsync(ct);
            try
            {
                // fast parse: everything before " ("
                var title = resource.Title ?? string.Empty;
                var idx = title.IndexOf(" (", StringComparison.Ordinal);
                var registration = (idx >= 0 ? title[..idx] : title).Trim();

                if (string.IsNullOrEmpty(registration))
                {
                    return;
                }

                _logger.LogDebug("Fetching maintenance data for {registration}", registration);

                var maintenanceData = await _techlogService.GetMaintenanceDataAsync(registration, day, ct);
                if (maintenanceData == null)
                {
                    return;
                }

                _logger.LogDebug(
                    "Maintenance data for {registration}: NextInspectionType={NextInspectionType}, TotalNextCheckHours={TotalNextCheckHours}",
                    registration,
                    maintenanceData.NextInspectionType,
                    maintenanceData.TotalNextCheckHours);

                // If SelectedDay changed while we were fetching, don't apply stale results
                if (SelectedDay != day)
                {
                    return;
                }

                var remaining = maintenanceData.TotalNextCheckHours.ToHoursMinutes();
                resource.Comment = string.IsNullOrEmpty(remaining) ? string.Empty : $"Next check in {remaining}";
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
                _logger.LogError(ex, "Failed to fetch maintenance data for {resourceTitle}", resource.Title);
            }
            finally
            {
                throttler.Release();
            }
        });

        await Task.WhenAll(tasks);
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
