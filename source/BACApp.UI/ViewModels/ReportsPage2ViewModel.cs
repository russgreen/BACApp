using BACApp.Core.Extensions;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using BACApp.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
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

internal partial class ReportsPage2ViewModel : PageViewModel
{
    private readonly ILogger<ReportsPage2ViewModel> _logger;
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
    private Aircraft _selectedAircraft;

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


    public ReportsPage2ViewModel(ILogger<ReportsPage2ViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        IFlightLogsService flightLogsService) : base(ApplicationPageNames.Reports2)
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
        int.TryParse(SelectedYearEnding,out int selectedYear);

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
            .ToList();

        if (AllAircraftList != null && AllAircraftList.Count > 0)
        {
            SelectedAircraft = AllAircraftList.First();
            await ReloadFlightLogsAsync(ct);
        }
    }

    async partial void OnSelectedAircraftChanged(Aircraft value)
    {
        await ReloadFlightLogsAsync(default);
    }

    async partial void OnSelectedYearEndingChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        SetDates();

        await ReloadFlightLogsAsync(default);
    }

    private async Task ReloadFlightLogsAsync(CancellationToken ct)
    {
        if (_authService.UserCompany is null || SelectedAircraft is null)
        {
            FlightLogs = new ObservableCollection<FlightLog>();
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
            var logs = await _flightLogsService.GetFlightLogsAsync(
                SelectedAircraft.Registration, from, to, ct);

            foreach (var log in logs)
            {
                log.SetChargeTime(SelectedAircraft.UseBrakesTimeToInvoice, SelectedAircraft.TimeAdjustMinutes);
            }

            var sorted = logs
                .OrderByDescending(x => x.FlightDate)
                .ThenByDescending(x => x.BrakesOffTime)
                .ToList();

            FlightLogs = new ObservableCollection<FlightLog>(sorted);

            //PopulateChartArrays(sorted);
            WireupChartSeries(sorted);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (JsonException)
        {
            FlightLogs = new ObservableCollection<FlightLog>();
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

    private void WireupChartSeries(List<FlightLog> sorted)
    {
        Series = new();

        double?[] cumulativeCurrentYear, cumulativePreviousYear;
        double[] monthlyCurrentYear, monthlyPreviousYear;
        //WireUpValueArrays(sorted, SelectedAircraft?.Registration, out cumulativeCurrentYear, out cumulativePreviousYear);
        //WireUpValueArrays(sorted, SelectedAircraft?.Registration, out monthlyCurrentYear, out monthlyPreviousYear, false);
        WireUpCumulativeValueArrays(sorted, SelectedAircraft?.Registration, out cumulativeCurrentYear, out cumulativePreviousYear);
        WireUpMonthlyValueArrays(sorted, SelectedAircraft?.Registration, out monthlyCurrentYear, out monthlyPreviousYear);

        //build the current year previous twelve months series
        var cumulativeCurrentSeries = new LineSeries<double?>()
        {
            Name = "Current year hours",
            Values = cumulativeCurrentYear,
            LineSmoothness = 0, //straight lines, no smoothing
        };

        Series.Add(cumulativeCurrentSeries);

        // NEW: projected trend line based on existing cumulative points (ignores nulls for fitting)
        var projectedTrend = BuildLinearTrendProjection(cumulativeCurrentYear);
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

        var monthlyCurrentSeries = new ColumnSeries<double>()
        {
            Name = "Current year hours",
            Values = monthlyCurrentYear,
        };

        Series.Add(monthlyCurrentSeries);


        //build the previoes year series for comparison
        var cumulativePreviousSeries = new LineSeries<double?>()
        {
            Name = "Preceeding year hours",
            Values = cumulativePreviousYear,
            LineSmoothness = 0, //straight lines, no smoothing
            Fill = null,
            Stroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 },
            GeometryStroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 }
        };

        Series.Add(cumulativePreviousSeries);

        var monthlyPreviousSeries = new ColumnSeries<double>()
        {
            Name = "Preceeding year hours",
            Values = monthlyPreviousYear,
            Fill = new SolidColorPaint(SKColors.Gray),
            Stroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 },
        };

        Series.Add(monthlyPreviousSeries);

    }

    private void WireUpValueArrays(List<FlightLog> sorted,
        string? registration,
        out double[] currentYear,
        out double[] previousYear,
        bool cumulativeTotals = true)
    {
        // Desired alignment:
        var now = DateTime.Now;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);

        // last12 window: 
        var last12Start = FromDate;

        // previous12 window:
        var previous12Start = last12Start.AddMonths(-12);

        currentYear = new double[12];
        previousYear = new double[12];

        // 1) Pre-seed from historical table (monthly totals)
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

            for (var i = 0; i < 12; i++)
            {
                var month = previous12Start.AddMonths(i);
                if (HistoricalMonthlyChargeHours.TryGetValue((registration, month), out var hours))
                {
                    previousYear[i] = hours;
                }
            }
        }

        // 2) Add system-recorded logs on top (so real data extends/overrides historical baselines)
        foreach (var log in sorted)
        {
            var monthStart = new DateTime(log.FlightDate.Year, log.FlightDate.Month, 1);
            var chargeHours = log.ChargeTimeDecimal;

            // Last 12 months (including current month): [last12Start, currentMonthStart + 1 month)
            if (monthStart >= last12Start && monthStart < currentMonthStart.AddMonths(1))
            {
                var index = (monthStart.Year - last12Start.Year) * 12 + (monthStart.Month - last12Start.Month);
                if ((uint)index < 12u)
                {
                    currentYear[index] += chargeHours;
                }

                continue;
            }

            // Previous 12 months: [previous12Start, last12Start)
            if (monthStart >= previous12Start && monthStart < last12Start)
            {
                var index = (monthStart.Year - previous12Start.Year) * 12 + (monthStart.Month - previous12Start.Month);
                if ((uint)index < 12u)
                {
                    previousYear[index] += chargeHours;
                }
            }
        }

        if (cumulativeTotals)
        {
            //// Convert monthly totals to cumulative totals (running sum)
            //for (var i = 1; i < 12; i++)
            //{
            //    currentYear[i] += currentYear[i - 1];
            //    previousYear[i] += previousYear[i - 1];
            //}

            // Only cumulate currentYear through the last "non-future" month.
            // Use the earlier of (ToDate month) and (current month).
            var latestAllowedMonthStart = new DateTime(
                Math.Min(ToDate.Year, now.Year),
                (ToDate.Year < now.Year) ? ToDate.Month : Math.Min(ToDate.Month, now.Month),
                1);

            var accumulateThroughIndex =
                (latestAllowedMonthStart.Year - last12Start.Year) * 12 +
                (latestAllowedMonthStart.Month - last12Start.Month);

            if (accumulateThroughIndex < 0) accumulateThroughIndex = -1;
            if (accumulateThroughIndex > 11) accumulateThroughIndex = 11;

            // Convert monthly totals to cumulative totals (running sum), but stop at accumulateThroughIndex.
            for (var i = 1; i <= accumulateThroughIndex; i++)
            {
                currentYear[i] += currentYear[i - 1];
            }

            // Ensure future months do not show cumulative carry-forward.
            for (var i = accumulateThroughIndex + 1; i < 12; i++)
            {
                currentYear[i] = 0;
            }

            // Previous year is always in the past relative to the selected range; cumulate full 12.
            for (var i = 1; i < 12; i++)
            {
                previousYear[i] += previousYear[i - 1];
            }
        }


    }

    private void WireUpMonthlyValueArrays(
    List<FlightLog> sorted,
    string? registration,
    out double[] currentYear,
    out double[] previousYear)
{
    var now = DateTime.Now;
    var currentMonthStart = new DateTime(now.Year, now.Month, 1);

    var last12Start = FromDate;
    var previous12Start = last12Start.AddMonths(-12);

    currentYear = new double[12];
    previousYear = new double[12];

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

        for (var i = 0; i < 12; i++)
        {
            var month = previous12Start.AddMonths(i);
            if (HistoricalMonthlyChargeHours.TryGetValue((registration, month), out var hours))
            {
                previousYear[i] = hours;
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

        if (monthStart >= previous12Start && monthStart < last12Start)
        {
            var index = (monthStart.Year - previous12Start.Year) * 12 + (monthStart.Month - previous12Start.Month);
            if ((uint)index < 12u)
            {
                previousYear[index] += chargeHours;
            }
        }
    }
}

private void WireUpCumulativeValueArrays(
    List<FlightLog> sorted,
    string? registration,
    out double?[] currentYear,
    out double?[] previousYear)
{
    // build from monthly so we keep the same seeding + log-merging logic
    WireUpMonthlyValueArrays(sorted, registration, out var monthlyCurrent, out var monthlyPrevious);

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
    previousYear = new double?[12];

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

    // previous year cumulative: always 12 points
    running = 0;
    for (var i = 0; i < 12; i++)
    {
        running += monthlyPrevious[i];
        previousYear[i] = running;
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
