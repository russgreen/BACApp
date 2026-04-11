using BACApp.Core.Extensions;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.ViewModels;

internal partial class ReportsPage3ViewModel : PageViewModel
{
    private readonly ILogger<ReportsPage3ViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IAircraftService _aircraftService;
    private readonly IFlightLogsService _flightLogsService;

    private CancellationTokenSource? _flightLogsCts;

    // Hard-coded monthly totals for months prior to (or missing from) system logs.
    // Key: (Registration, MonthStart) where MonthStart is the 1st of the month at 00:00.
    private static readonly IReadOnlyDictionary<(string Registration, DateTime MonthStart), double> HistoricalMonthlyChargeHours =
        BACApp.UI.HistoricData.BuildHistoricalMonthlyChargeHours();

    [ObservableProperty]
    private List<Aircraft> _allAircraftList;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private ObservableCollection<FlightLog> _flightLogs;

    [ObservableProperty]
    private string[] _labels = new string[12];

    [ObservableProperty]
    private ObservableCollection<ISeries> _series;

    [ObservableProperty]
    private ObservableCollection<Axis> _xAxis;

    public ReportsPage3ViewModel(ILogger<ReportsPage3ViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        IFlightLogsService flightLogsService) : base(ApplicationPageNames.Reports3)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _flightLogsService = flightLogsService;

        // Keep chart aligned to rolling last 12 months (including current month).
        FromDate = DateTime.Now.AddMonths(-11);
        ToDate = DateTime.Now;

        WireupAxisLabels();

        // Defer async work; do not block constructor
        LoadAsync().ConfigureAwait(false);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        if (_authService.UserCompany is null)
        {
            return;
        }

        AllAircraftList = _aircraftService.AllCompanyAircraft
        .OrderBy(a => a.Registration)
        .Where(a => a.Registration != "G-ARKS")
        .ToList();

        if (AllAircraftList != null && AllAircraftList.Count > 0)
        {
            await LoadFlightLogsAsync(ct);
        }
            
    }

    private void WireupAxisLabels()
    {
        var orderMonthNames = new string[12];

        for (var i = 0; i < 12; i++)
        {
            var month = DateTime.Now.AddMonths(-11 + i);
            var monthName = month.ToString("MMM");
            _logger.LogDebug("{sequence} : {monthno} : {monthname}", i, month.Month, monthName);

            orderMonthNames[i] = monthName;
        }

        XAxis = new()
        {
            new Axis
            {
                Labels = orderMonthNames
            }
        };

    }

    private async Task LoadFlightLogsAsync(CancellationToken ct)
    {
        if (_authService.UserCompany is null || AllAircraftList is null || AllAircraftList.Count == 0)
        {
            FlightLogs = new ObservableCollection<FlightLog>();
            Series = new ObservableCollection<ISeries>();
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
            var validAircraft = AllAircraftList
                .Where(a => !string.IsNullOrWhiteSpace(a.Registration))
                .ToList();

            var tasks = validAircraft.Select(async aircraft =>
            {
                var logs = await _flightLogsService.GetFlightLogsAsync(aircraft.Registration, from, to, ct);
                var list = logs.ToList();

                return (Aircraft: aircraft, Logs: list);
            });

            var perAircraft = await Task.WhenAll(tasks);

            var sortedAllLogs = perAircraft
                .SelectMany(x => x.Logs)
                .OrderByDescending(x => x.FlightDate)
                .ThenByDescending(x => x.BrakesOffTime)
                .ToList();

            FlightLogs = new ObservableCollection<FlightLog>(sortedAllLogs);

            WireupAxisLabels();
            WireupChartSeries(perAircraft);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (JsonException)
        {
            FlightLogs = new ObservableCollection<FlightLog>();
            Series = new ObservableCollection<ISeries>();
        }
    }

    private void WireupChartSeries((Aircraft Aircraft, List<FlightLog> Logs)[] perAircraft)
    {
        Series = new ObservableCollection<ISeries>();

        var now = DateTime.Now;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var windowStart = currentMonthStart.AddMonths(-11);
        var totalAllAircraft = new double[12];

        foreach (var item in perAircraft.OrderBy(x => x.Aircraft.Registration))
        {
            var monthly = BuildMonthlySeries(item.Aircraft, item.Logs, windowStart);

            for (var i = 0; i < 12; i++)
            {
                totalAllAircraft[i] += monthly[i];
            }

            Series.Add(new LineSeries<double>
            {
                Name = item.Aircraft.Registration,
                Values = monthly,
                LineSmoothness = 0
            });
        }

        Series.Add(new LineSeries<double>
        {
            Name = "All aircraft total",
            Values = totalAllAircraft,
            LineSmoothness = 0
        });
    }

    private static double[] BuildMonthlySeries(Aircraft aircraft, List<FlightLog> logs, DateTime windowStart)
    {
        var values = new double[12];

        // historical seed for this 12-month window
        for (var i = 0; i < 12; i++)
        {
            var monthStart = windowStart.AddMonths(i);
            if (HistoricalMonthlyChargeHours.TryGetValue((aircraft.Registration, monthStart), out var historicalHours))
            {
                values[i] = historicalHours;
            }
        }

        // add API/system log values
        foreach (var log in logs)
        {
            var monthStart = new DateTime(log.FlightDate.Year, log.FlightDate.Month, 1);
            var index = (monthStart.Year - windowStart.Year) * 12 + (monthStart.Month - windowStart.Month);

            if ((uint)index < 12u)
            {
                values[index] += log.ChargeTimeDecimal;
            }
        }

        // convert monthly values to running totals
        for (var i = 1; i < values.Length; i++)
        {
            values[i] += values[i - 1];
        }

        return values;
    }
}
