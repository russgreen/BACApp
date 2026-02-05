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

internal partial class ReportsPageViewModel : PageViewModel
{
    private readonly ILogger<ReportsPageViewModel> _logger;
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


    public ReportsPageViewModel(ILogger<ReportsPageViewModel> logger,
        IAuthService authService,
        IAircraftService aircraftService,
        IFlightLogsService flightLogsService) : base(ApplicationPageNames.Reports)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _flightLogsService = flightLogsService;

        FromDate = DateTime.Now.AddMonths(-24);
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

    private void WireupChartSeries(List<FlightLog> sorted)
    {
        Series = new();

        double[] cumulativeLast12, cumulativePrevious12, monthlyLast12, monthlyPrevious12;
        WireUpValueArrays(sorted, SelectedAircraft?.Registration, out cumulativeLast12, out cumulativePrevious12);
        WireUpValueArrays(sorted, SelectedAircraft?.Registration, out monthlyLast12, out monthlyPrevious12, false);

        //build the current year previous twelve months series
        var cumumulativeLast12Series = new LineSeries<double>()
        {
            Name = "Current year hours",
            Values = cumulativeLast12,
            LineSmoothness = 0, //straight lines, no smoothing
        };

        Series.Add(cumumulativeLast12Series);

        var monthlyLast12Series = new ColumnSeries<double>()
        {
            Name = "Current year hours",
            Values = monthlyLast12,
        };

        Series.Add(monthlyLast12Series);


        //build the previoes year series for comparison
        var cumulativePrevious12Series = new LineSeries<double>()
        {
            Name = "Preceeding year hours",
            Values = cumulativePrevious12,
            LineSmoothness = 0, //straight lines, no smoothing
            Fill = null,
            Stroke  = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 },
            GeometryStroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 }
        };

        Series.Add(cumulativePrevious12Series);

        var monthlyPrevious12Series = new ColumnSeries<double>()
        {
            Name = "Preceeding year hours",
            Values = monthlyPrevious12,
            Fill = new SolidColorPaint(SKColors.Gray),
            Stroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 },
        };

        Series.Add(monthlyPrevious12Series);

    }

    private static void WireUpValueArrays(List<FlightLog> sorted, 
        string? registration, 
        out double[] last12, 
        out double[] previous12,
        bool cumulativeTotals = true)
    {
        // Desired alignment:
        // If current month is Jan 2026:
        //  - last12[0]  => Feb 2025
        //  - last12[11] => Jan 2026 (current month)
        var now = DateTime.Now;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);

        // last12 window: [currentMonthStart - 11 months, currentMonthStart] (month buckets)
        var last12Start = currentMonthStart.AddMonths(-11);

        // previous12 window: the 12 months immediately before last12Start
        var previous12Start = last12Start.AddMonths(-12);

        last12 = new double[12];
        previous12 = new double[12];

        // 1) Pre-seed from historical table (monthly totals)
        if (!string.IsNullOrWhiteSpace(registration))
        {
            for (var i = 0; i < 12; i++)
            {
                var month = last12Start.AddMonths(i);
                if (HistoricalMonthlyChargeHours.TryGetValue((registration, month), out var hours))
                {
                    last12[i] = hours;
                }
            }

            for (var i = 0; i < 12; i++)
            {
                var month = previous12Start.AddMonths(i);
                if (HistoricalMonthlyChargeHours.TryGetValue((registration, month), out var hours))
                {
                    previous12[i] = hours;
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
                    last12[index] += chargeHours;
                }

                continue;
            }

            // Previous 12 months: [previous12Start, last12Start)
            if (monthStart >= previous12Start && monthStart < last12Start)
            {
                var index = (monthStart.Year - previous12Start.Year) * 12 + (monthStart.Month - previous12Start.Month);
                if ((uint)index < 12u)
                {
                    previous12[index] += chargeHours;
                }
            }
        }

        if (cumulativeTotals)
        {
            // Convert monthly totals to cumulative totals (running sum)
            for (var i = 1; i < 12; i++)
            {
                last12[i] += last12[i - 1];
                previous12[i] += previous12[i - 1];
            }
        }


    }

}

