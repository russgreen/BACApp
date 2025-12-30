using BACApp.Core.Models;

namespace BACApp.Core.Services;

public class FlightLogsService : IFlightLogsService
{
    private readonly Abstractions.IApiClient _apiClient;

    public FlightLogsService(Abstractions.IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(int companyId, string registration, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = companyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration
        };

        var result = await _apiClient.GetAsync<List<FlightLog>>("/flightlog/list/byAircraftFilters", query, headers, ct);
        return result ?? new List<FlightLog>();
    }

    public async Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(int companyId, string registration, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = companyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration,
            ["date_from"] = from.ToString("yyyy-MM-dd"),
            ["date_to"] = to.ToString("yyyy-MM-dd")
        };

        var result = await _apiClient.GetAsync<List<FlightLog>>("/flightlog/list/byAircraftFilters",  query, headers, ct);
        return result ?? new List<FlightLog>();
    }


}
