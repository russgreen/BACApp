using BACApp.Core.DTO;
using BACApp.Core.Models;
using BACApp.Core.Services;
using BACApp.UI.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.ViewModels;

internal partial class LogsPageViewModel : PageViewModel
{
    private readonly ILogger<LogsPageViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly IAircraftService _aircraftService;
    private readonly IFlightLogsService _flightLogsService;
    private readonly ICsvExportService _csvExportService;

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
    private List<FlightLog> _allFlightLogs;

    [ObservableProperty]
    private ObservableCollection<FlightLog> _filteredFlightLogs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogsSelected))]
    private ObservableCollection<FlightLog> _selectedFlightLogs;

    public bool HasLogsSelected => SelectedFlightLogs != null && SelectedFlightLogs.Count > 0;

    public LogsPageViewModel(ILogger<LogsPageViewModel>logger,
        IAuthService authService,
        IAircraftService aircraftService,
        IFlightLogsService flightLogsService,
        ICsvExportService csvExportService) 
        : base(ApplicationPageNames.Logs)
    {
        _logger = logger;
        _authService = authService;
        _aircraftService = aircraftService;
        _flightLogsService = flightLogsService;
        _csvExportService = csvExportService;

        FromDate = DateTime.Now.AddMonths(-1);
        ToDate = DateTime.Now;

        SelectedFlightLogs = new ObservableCollection<FlightLog>();
        FilteredFlightLogs = new ObservableCollection<FlightLog>();
        AllFlightLogs = new List<FlightLog>();

        // Defer async work; do not block constructor
        LoadAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void ExportLogs()
    {
        // Example default export location. You can replace this with a SaveFile dialog from the View if desired.
        //TODO handle output in a cross platform way
        var defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var aircraftReg = SelectedAircraft?.Registration ?? "unknown";
        var fileName = $"FlightLogs_{aircraftReg}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var fullPath = Path.Combine(defaultDir, fileName);

        _csvExportService.Export(SelectedFlightLogs, fullPath);
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
        }
    }


    partial void OnSelectedAircraftChanged(Aircraft value)
    {
        LoadAllFlightLogsAsync();
    }

    partial void OnFromDateChanged(DateTime value)
    {
        ApplyFilter();
    }

    partial void OnToDateChanged(DateTime value)
    {
        ApplyFilter();
    }

    private async Task LoadAllFlightLogsAsync(CancellationToken ct = default)
    {
        if (_authService.UserCompany is null || SelectedAircraft is null)
        {
            AllFlightLogs = new List<FlightLog>();
            ApplyFilter();
            return;
        }

        try
        {
            var logs = await _flightLogsService.GetFlightLogsAsync(
                 SelectedAircraft.Registration, ct);

            AllFlightLogs = logs
                .OrderByDescending(x => x.FlightDate)
                .ThenByDescending(x => x.BrakesOffTime)
                .ToList();

            ApplyFilter();
        }
        catch (OperationCanceledException) { /* ignore canceled loads */ }
        catch (JsonException ex)
        {
            // TODO: log response body or switch to a DTO matching server payload
            AllFlightLogs = new List<FlightLog>();
            ApplyFilter();
        }
    }

    private void ApplyFilter()
    {
        var source = AllFlightLogs;
        if (source is null || source.Count == 0)
        {
            // Replace with empty to trigger a single reset on the UI
            FilteredFlightLogs = new ObservableCollection<FlightLog>();
            return;
        }

        var from = FromDate.Date;
        var to = ToDate.Date;

        // Ensure proper bounds (swap if needed)
        if (from > to)
        {
            var tmp = from;
            from = to;
            to = tmp;
        }

        // Single pass filter: avoids multiple enumerations, minimizes allocations
        var result = new List<FlightLog>(capacity: source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            var log = source[i];
            var d = log.FlightDate.Date;
            if (d >= from && d <= to)
            {
                result.Add(log);
            }
        }

        // Replace collection to emit a single reset for the DataGrid
        FilteredFlightLogs = new ObservableCollection<FlightLog>(result);
    }

}
