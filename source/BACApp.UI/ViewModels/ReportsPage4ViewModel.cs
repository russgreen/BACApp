using BACApp.Core.Extensions;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BACApp.UI.ViewModels;

internal partial class ReportsPage4ViewModel : PageViewModel
{
    private readonly ILogger<ReportsPage4ViewModel> _logger;
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
    private List<string> _yearEndings = new();

    [ObservableProperty]
    private string _selectedYearEnding;

    [ObservableProperty]
    private DateTime _fromDate;

    [ObservableProperty]
    private DateTime _toDate;

    [ObservableProperty]
    private ObservableCollection<FlightLog> _flightLogs;

    [ObservableProperty]
    private double[] _totalLast12Months = new double[12];

    [ObservableProperty]
    private double[] _totalPrevious12Months = new double[12];

    [ObservableProperty]
    private string[] _labels = new string[12];

    [ObservableProperty]
    private ObservableCollection<ISeries> _series;

    [ObservableProperty]
    private ObservableCollection<Axis> _xAxis;

    public ReportsPage4ViewModel(ILogger<ReportsPage4ViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        IFlightLogsService flightLogsService) : base(ApplicationPageNames.Reports4)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _flightLogsService = flightLogsService;

        YearEndings = Enumerable.Range(DateTime.Now.Year - 1, 3)
            .Select(y => $"{y}")
            .ToList();

        SelectedYearEnding = YearEndings[1];

        SetDates();

        WireupAxisLabels();

        // Defer async work; do not block constructor
        LoadAsync().ConfigureAwait(false);
    }

    private void SetDates()
    {
        int.TryParse(SelectedYearEnding, out int selectedYear);

        FromDate = new DateTime(selectedYear - 1, 06, 01);
        ToDate = new DateTime(selectedYear, 05, 31);
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

    async partial void OnSelectedYearEndingChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        SetDates();

        await LoadFlightLogsAsync(default);
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

                foreach (var log in list)
                {
                    log.SetChargeTime(aircraft.UseBrakesTimeToInvoice, aircraft.TimeAdjustMinutes);
                }

                return (Aircraft: aircraft, Logs: list);
            });

            var perAircraft = await Task.WhenAll(tasks);

            var sortedAllLogs = perAircraft
                .SelectMany(x => x.Logs)
                .OrderByDescending(x => x.FlightDate)
                .ThenByDescending(x => x.BrakesOffTime)
                .ToList();

            FlightLogs = new ObservableCollection<FlightLog>(sortedAllLogs);

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

    private void WireupAxisLabels()
    {
        var orderMonthNames = new string[12];

        for (var i = 0; i < 12; i++)
        {
            var month = FromDate.AddMonths(i);
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

    private void WireupChartSeries((Aircraft Aircraft, List<FlightLog> Logs)[] perAircraft)
    {
        Series = new ObservableCollection<ISeries>();

        var totalAllAircraft = new double?[12];

        foreach (var item in perAircraft.OrderBy(x => x.Aircraft.Registration))
        {
            double?[] cumulativeAircraft;
            double[] monthlyAircraft;

            WireUpCumulativeValueArrays(item.Logs, item.Aircraft?.Registration, out cumulativeAircraft);
            WireUpMonthlyValueArrays(item.Logs, item.Aircraft?.Registration, out monthlyAircraft);

            // add cumulativeAircraft to totalAllAircraft and preserve null values unless
            // totalAllAircraft already has a non-null value (from a previous aircraft) for that month
            for (var i = 0; i < 12; i++)
            {
                var cumulativeValue = cumulativeAircraft[i];

                if (cumulativeValue is null)
                {
                    // keep null if total is still null; otherwise preserve existing non-null total
                    continue;
                }

                totalAllAircraft[i] = (totalAllAircraft[i] ?? 0d) + cumulativeValue.Value;
            }

            Series.Add(new LineSeries<double?>
            {
                Name = item.Aircraft.Registration,
                Values = cumulativeAircraft,
                LineSmoothness = 0
            });

            Series.Add(new ColumnSeries<double>()
            {
                Name = item.Aircraft.Registration,
                Values = monthlyAircraft,
            });

        }

        Series.Add(new LineSeries<double?>
        {
            Name = "Total All Aircraft",
            Values = totalAllAircraft,
            LineSmoothness = 0
        });

        // NEW: projected trend line based on existing cumulative points (ignores nulls for fitting)
        var projectedTrend = BuildLinearTrendProjection(totalAllAircraft);
        var projectedTrendSeries = new LineSeries<double?>()
        {
            Name = "Current year trend (projected)",
            Values = projectedTrend,
            LineSmoothness = 0,
            Fill = null,
            Stroke = new SolidColorPaint(SKColors.OrangeRed) { StrokeThickness = 2 },
            GeometryStroke = null,
            GeometrySize = 0
        };
        Series.Add(projectedTrendSeries);
    }

    private void WireUpMonthlyValueArrays(
List<FlightLog> sorted,
string? registration,
out double[] currentYear)
    {
        var now = DateTime.Now;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);

        var last12Start = FromDate;

        currentYear = new double[12];

        if (!string.IsNullOrWhiteSpace(registration))
        {
            for (var i = 0; i < 12; i++)
            {
                var month = last12Start.AddMonths(i);
                if (HistoricalMonthlyChargeHours.TryGetValue((registration, month), out var hours))
                {
                    currentYear[i] = hours;
                }
            }
        }

        foreach (var log in sorted)
        {
            var monthStart = new DateTime(log.FlightDate.Year, log.FlightDate.Month, 1);
            var chargeHours = log.ChargeTimeDecimal;

            if (monthStart >= last12Start && monthStart < currentMonthStart.AddMonths(1))
            {
                var index = (monthStart.Year - last12Start.Year) * 12 + (monthStart.Month - last12Start.Month);
                if ((uint)index < 12u)
                {
                    currentYear[index] += chargeHours;
                }

                continue;
            }
        }
    }

    private void WireUpCumulativeValueArrays(
        List<FlightLog> sorted,
        string? registration,
        out double?[] currentYear)
    {
        // build from monthly so we keep the same seeding + log-merging logic
        WireUpMonthlyValueArrays(sorted, registration, out var monthlyCurrent);

        var now = DateTime.Now;
        var last12Start = FromDate;

        // last month we want to show a cumulative point for (inclusive)
        var latestAllowedMonthStart = new DateTime(
            Math.Min(ToDate.Year, now.Year),
            (ToDate.Year < now.Year) ? ToDate.Month : Math.Min(ToDate.Month, now.Month),
            1);

        var accumulateThroughIndex =
            (latestAllowedMonthStart.Year - last12Start.Year) * 12 +
            (latestAllowedMonthStart.Month - last12Start.Month);

        if (accumulateThroughIndex < 0) accumulateThroughIndex = -1;
        if (accumulateThroughIndex > 11) accumulateThroughIndex = 11;

        currentYear = new double?[12];

        // current year cumulative: stop (null) after accumulateThroughIndex
        double running = 0;
        for (var i = 0; i < 12; i++)
        {
            if (i > accumulateThroughIndex)
            {
                currentYear[i] = null; // causes the line to stop instead of dropping to 0
                continue;
            }

            running += monthlyCurrent[i];
            currentYear[i] = running;
        }

    }

    private double?[] BuildLinearTrendProjection(double?[] cumulative)
    {
        if (cumulative.Length != 12)
        {
            throw new ArgumentException("Expected 12 months of data.", nameof(cumulative));
        }

        // Fit y = a + b*x using least squares over non-null points.
        // Mimics Excel trendline behavior on a category axis (x = 1..N).
        double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
        var n = 0;

        var lastNonNullIndex = -1;

        for (var i = 0; i < cumulative.Length; i++)
        {
            var y = cumulative[i];
            if (!y.HasValue)
            {
                continue;
            }

            // Excel category axis uses 1-based x positions.
            var x = (double)(i + 1);

            n++;
            lastNonNullIndex = i;

            sumX += x;
            sumY += y.Value;
            sumXX += x * x;
            sumXY += x * y.Value;
        }

        if (n < 2)
        {
            return new double?[12];
        }

        var denom = (n * sumXX) - (sumX * sumX);
        if (Math.Abs(denom) < 1e-9)
        {
            return new double?[12];
        }

        var b = ((n * sumXY) - (sumX * sumY)) / denom; // slope
        var a = (sumY - (b * sumX)) / n;               // intercept

        var trend = new double?[12];

        for (var i = 0; i < 12; i++)
        {
            // Preserve null gaps inside the known-data region; project after the last known point.
            if (i <= lastNonNullIndex && !cumulative[i].HasValue)
            {
                trend[i] = null;
                continue;
            }

            var x = (double)(i + 1);
            var y = a + (b * x);

            trend[i] = y;
        }

        return trend;
    }
}
