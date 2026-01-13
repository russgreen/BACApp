using BACApp.Core.DTO;
using BACApp.Core.Extensions;
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
    private ObservableCollection<FlightLog> _filteredFlightLogs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogsSelected))]
    private ObservableCollection<FlightLog> _selectedFlightLogs;

    public bool HasLogsSelected => SelectedFlightLogs != null && SelectedFlightLogs.Count > 0;

    public Func<Task<string?>>? PickExportFilePathAsync { get; set; }

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
        //var fileName = $"FlightLogs_{aircraftReg}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        //var fullPath = Path.Combine(preferredDir, fileName);

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
            await ReloadFlightLogsAsync(ct);
        }
    }

    [RelayCommand]
    private async Task ReloadFlightLogsAsync(CancellationToken ct)
    {
        if (_authService.UserCompany is null || SelectedAircraft is null)
        {
            FilteredFlightLogs = new ObservableCollection<FlightLog>();
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

            FilteredFlightLogs = new ObservableCollection<FlightLog>(sorted);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (JsonException)
        {
            FilteredFlightLogs = new ObservableCollection<FlightLog>();
        }
    }

}
