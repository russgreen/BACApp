using Avalonia.Collections;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using BACApp.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.ViewModels;

internal partial class LogsAirframePageViewModel : PageViewModel
{
    private readonly ILogger<LogsAirframePageViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IAircraftService _aircraftService;
    private readonly IFlightLogsService _flightLogsService;
    private readonly ITechlogService _techlogService;
    private readonly ICsvExportService _csvExportService;

    private readonly Dictionary<(string Registration, DateOnly Date), TimeSpan?> _airframeTotalCache = new();

    private CancellationTokenSource? _flightLogsCts;

    [ObservableProperty]
    private List<Aircraft> _allAircraftList;

    [ObservableProperty]
    private Aircraft _selectedAircraft;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDailySummaries))]
    private ObservableCollection<AirframeDailySummary> _dailySummaries;

    public bool HasDailySummaries => DailySummaries != null && DailySummaries.Count > 0;

    public Func<Task<string?>>? PickExportFilePathAsync { get; set; }

    public LogsAirframePageViewModel(ILogger<LogsAirframePageViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        IFlightLogsService flightLogsService,
        ITechlogService techlogService,
        ICsvExportService csvExportService) : base(ApplicationPageNames.TechLogs)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _flightLogsService = flightLogsService;
        _techlogService = techlogService;
        _csvExportService = csvExportService;

        FromDate = DateTime.Now.AddMonths(-1);
        ToDate = DateTime.Now;

        DailySummaries = new ObservableCollection<AirframeDailySummary>();

        // Defer async work; do not block constructor
        LoadAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task ExportLogsAsync()
    {
        // Example default export location. You can replace this with a SaveFile dialog from the View if desired.
        //TODO handle output in a cross platform way
        var picker = PickExportFilePathAsync;
        if (picker is null)
        {
            _logger.LogWarning("Export requested but no file picker delegate is configured.");
            return;
        }

        var fullPath = await picker();
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return;
        }

        //var aircraftReg = SelectedAircraft?.Registration ?? "unknown";
        //var fileName = $"AirframeTechLog_{aircraftReg}_{FromDate:yyyyMMdd}-{ToDate:yyyyMMdd}.csv";
        //var fullPath = Path.Combine(path, fileName);

        _csvExportService.Export(DailySummaries, fullPath);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        if (_authService.UserCompany is null)
        {
            return;
        }

        AllAircraftList = _aircraftService.AllCompanyAircraft
            .OrderBy(a => a.Registration)
            .ToList();

        if (AllAircraftList != null && AllAircraftList.Count > 0)
        {
            SelectedAircraft = AllAircraftList.First();
            await ReloadFlightLogsAsync(ct);
        }
    }


    [RelayCommand]
    private async Task ReloadFlightLogsAsync(CancellationToken ct)
    {
        if (_authService.UserCompany is null || SelectedAircraft is null)
        {
            DailySummaries = new ObservableCollection<AirframeDailySummary>();
            return;
        }

        var from = DateOnly.FromDateTime(FromDate);
        var to = DateOnly.FromDateTime(ToDate);

        if (from > to)
        {
            (from, to) = (to, from);
        }

        try
        {
            // 1 API call for the whole range
            var logs = await _flightLogsService.GetFlightLogsAsync(SelectedAircraft.Registration, from, to, ct);

            // Group into one row per day
            var groups = logs
                .GroupBy(l => DateOnly.FromDateTime(l.FlightDate))
                .OrderBy(g => g.Key)
                .ToList();

            // Collect dates we actually need maintenance totals for (minimizes calls).
            var dates = groups.Select(g => g.Key).ToList();

            // N calls for maintenance data where N = number of displayed days (cached across reloads).
            var airframeTotalsByDate = await GetEndOfDayAirframeTotalsByDateAsync(
                SelectedAircraft.Registration,
                dates,
                ct);

            var rows = new List<AirframeDailySummary>(capacity: groups.Count);
            foreach (var g in groups)
            {
                // Use your existing computed property; add exact durations.
                var totalFlightTime = TimeSpan.FromTicks(g.Sum(x => x.FlightTime.Ticks));

                rows.Add(new AirframeDailySummary
                {
                    Date = g.Key,
                    FlightCount = g.Count(),
                    TotalFlightTime = totalFlightTime,
                    EndOfDayAirframeTotal = airframeTotalsByDate.TryGetValue(g.Key, out var total) ? total : null
                });
            }

            DailySummaries = new ObservableCollection<AirframeDailySummary>(rows);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (JsonException)
        {
            DailySummaries = new ObservableCollection<AirframeDailySummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload airframe daily summaries.");
            DailySummaries = new ObservableCollection<AirframeDailySummary>();
        }
    }

    private async Task<Dictionary<DateOnly, TimeSpan?>> GetEndOfDayAirframeTotalsByDateAsync(
        string registration,
        IReadOnlyList<DateOnly> dates,
        CancellationToken ct)
    {
        var results = new Dictionary<DateOnly, TimeSpan?>();

        var missing = new List<DateOnly>();
        foreach (var date in dates)
        {
            if (_airframeTotalCache.TryGetValue((registration, date), out var cached))
            {
                results[date] = cached;
                continue;
            }

            missing.Add(date);
        }

        if (missing.Count == 0)
        {
            return results;
        }

        var tasks = missing.Select(async date =>
        {
            try
            {
                var data = await _techlogService.GetMaintenanceDataAsync(registration, date, ct);
                var total = TryParseTotalAirframeHours(data?.TotalAirframeHours);
                _airframeTotalCache[(registration, date)] = total;
                return (Date: date, Total: total);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Maintenance data fetch failed for {Registration} on {Date}", registration, date);
                _airframeTotalCache[(registration, date)] = null;
                return (Date: date, Total: (TimeSpan?)null);
            }
        });

        foreach (var item in await Task.WhenAll(tasks))
        {
            results[item.Date] = item.Total;
        }

        return results;
    }

    private static TimeSpan? TryParseTotalAirframeHours(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Trim().Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return null;
        }

        if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var hours))
        {
            return null;
        }

        if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minutes))
        {
            return null;
        }

        if (!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
        {
            return null;
        }

        if (minutes is < 0 or > 59 || seconds is < 0 or > 59 || hours < 0)
        {
            return null;
        }

        return new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0).Add(TimeSpan.FromHours(hours))
            .Add(TimeSpan.FromMinutes(minutes))
            .Add(TimeSpan.FromSeconds(seconds));
    }
}
