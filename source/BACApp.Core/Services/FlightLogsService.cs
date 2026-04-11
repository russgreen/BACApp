using BACApp.Core.Abstractions;
using BACApp.Core.Extensions;
using BACApp.Core.Models;

namespace BACApp.Core.Services;

public class FlightLogsService : IFlightLogsService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly IInvoiceService _invoiceService;
    private readonly IAircraftService _aircraftService;

    public FlightLogsService(IApiClient apiClient,
        IAuthService authService,
        IInvoiceService invoiceService,
        IAircraftService aircraftService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _invoiceService = invoiceService;
        _aircraftService = aircraftService;
    }

    public async Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(string registration, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration
        };

        var path = "/flightlog/list/byAircraftFilters";
        var data = await _apiClient.GetAsync<List<FlightLog>>(path, query, headers, ct);

        var aircraft = _aircraftService.AllCompanyAircraft.Where(x => x.Registration == registration).First();

        foreach (var log in data)
        {
            log.SetChargeTime(aircraft.UseBrakesTimeToInvoice, aircraft.TimeAdjustMinutes);
        }

        return data ?? new List<FlightLog>();
    }

    public async Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(string registration, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration,
            ["dateFrom"] = from.ToString("yyyy-MM-dd"),
            ["dateTo"] = to.ToString("yyyy-MM-dd")
        };

        var path = "/flightlog/list/byAircraftFilters";
        var data = await _apiClient.GetAsync<List<FlightLog>>(path,  query, headers, ct);

        var aircraft = _aircraftService.AllCompanyAircraft.Where(x => x.Registration == registration).First();

        foreach (var log in data)
        {
            log.SetChargeTime(aircraft.UseBrakesTimeToInvoice, aircraft.TimeAdjustMinutes);
        }

        return data ?? new List<FlightLog>();
    }

    public async Task<IReadOnlyList<FlightLog>> GetAllFlightLogsAsync(CancellationToken ct = default)
    {
        var logsByAircraftTasks = _aircraftService.AllCompanyAircraft
            .Select(aircraft => GetFlightLogsAsync(aircraft.Registration, ct))
            .ToList();

        var logsByAircraft = await Task.WhenAll(logsByAircraftTasks);

        return logsByAircraft.SelectMany(x => x).ToList();
    }

    public async Task<IReadOnlyList<FlightLog>> GetFlightLogsUnbilledAsync(CancellationToken ct = default)
    {
        var allInvoicesTask = _invoiceService.GetAllInvoicesAsync(ct);
        var allFlightLogsTask = GetAllFlightLogsAsync(ct);

        await Task.WhenAll(allInvoicesTask, allFlightLogsTask);

        var referenceIds = new HashSet<int>(allInvoicesTask.Result
            .SelectMany(i => i.InvoiceLines ?? Enumerable.Empty<InvoiceLine>())
            .Where(l => l.FlightLogId.HasValue)
            .Select(l => l.FlightLogId!.Value));

        return allFlightLogsTask.Result
            .Where(flightLog => !referenceIds.Contains(flightLog.FlightLogId))
            .ToList();
    }
}
