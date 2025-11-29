using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface IReportsService
{
    Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(string aircraftId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<TechLog>> GetTechLogsAsync(string aircraftId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public class ReportsService : IReportsService
{
    private readonly Abstractions.IApiClient _apiClient;

    public ReportsService(Abstractions.IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(string aircraftId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["aircraft_id"] = aircraftId,
            ["from"] = from.ToString("yyyy-MM-dd"),
            ["to"] = to.ToString("yyyy-MM-dd")
        };

        var result = await _apiClient.GetAsync<List<FlightLog>>("/flightlog/list/byAircraftFilters", query, ct);
        return result ?? new List<FlightLog>();
    }

    public async Task<IReadOnlyList<TechLog>> GetTechLogsAsync(string aircraftId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["aircraft_id"] = aircraftId,
            ["from"] = from.ToString("yyyy-MM-dd"),
            ["to"] = to.ToString("yyyy-MM-dd")
        };

        var result = await _apiClient.GetAsync<List<TechLog>>("/techlog/list/GetDTLFlightsByAircraft", query, ct);
        return result ?? new List<TechLog>();
    }
}
