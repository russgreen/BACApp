using BACApp.Core.Extensions;
using BACApp.Core.Helpers;
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

        YearEndings = YearEndingsHelper.GetYearEndings();

        SelectedYearEnding = YearEndingsHelper.CurrentYearEnding(YearEndings);

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

        var from = DateOnly.FromDateTime(FromDate.AddMonths(-12));
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

        // For a selected year-end that extends into the future, show actual data only up to the
        // current month. For a historical year-end, keep the selected end-month boundary.
        var latestAllowedMonthStart = ToDate.Year > now.Year
            ? new DateTime(now.Year, now.Month, 1)
            : new DateTime(
                Math.Min(ToDate.Year, now.Year),
                (ToDate.Year < now.Year) ? ToDate.Month : Math.Min(ToDate.Month, now.Month),
                1);

        var accumulateThroughIndex =
            (latestAllowedMonthStart.Year - last12Start.Year) * 12 +
            (latestAllowedMonthStart.Month - last12Start.Month);

        if (accumulateThroughIndex < 0) accumulateThroughIndex = -1;
        if (accumulateThroughIndex > 11) accumulateThroughIndex = 11;

        currentYear = new double?[12];

        // Do not draw cumulative values for future months. The current-year line should stop at
        // the last actual month in the current period so the projection line is based on real data
        // and not artificially flattened by future months.
        double running = 0;
        for (var i = 0; i < 12; i++)
        {
            if (i > accumulateThroughIndex)
            {
                currentYear[i] = null;
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

        var validPoints = new List<(int Index, double Value)>();
        for (var i = 0; i < cumulative.Length; i++)
        {
            if (cumulative[i].HasValue)
            {
                validPoints.Add((i, cumulative[i].Value));
            }
        }

        var trend = new double?[12];
        if (validPoints.Count == 0)
        {
            return trend;
        }

        if (validPoints.Count == 1)
        {
            var only = validPoints[0];
            for (var i = only.Index; i < trend.Length; i++)
            {
                trend[i] = only.Value;
            }
            return trend;
        }

        double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
        foreach (var point in validPoints)
        {
            var x = (double)(point.Index + 1);
            sumX += x;
            sumY += point.Value;
            sumXX += x * x;
            sumXY += x * point.Value;
        }

        var n = validPoints.Count;
        var denom = (n * sumXX) - (sumX * sumX);
        if (Math.Abs(denom) < 1e-9)
        {
            var lastValue = validPoints[^1].Value;
            for (var i = validPoints[^1].Index; i < trend.Length; i++)
            {
                trend[i] = lastValue;
            }
            return trend;
        }

        var b = ((n * sumXY) - (sumX * sumY)) / denom;
        var a = (sumY - (b * sumX)) / n;

        var firstValidIndex = validPoints[0].Index;
        var lastValidIndex = validPoints[^1].Index;

        for (var i = 0; i < 12; i++)
        {
            if (i < firstValidIndex)
            {
                trend[i] = null;
                continue;
            }

            if (i <= lastValidIndex && !cumulative[i].HasValue)
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
