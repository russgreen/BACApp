using BACApp.Core.Abstractions;
using BACApp.Core.Models;

namespace BACApp.Core.Services;

public class FlightLogsService : IFlightLogsService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public FlightLogsService(IApiClient apiClient,
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
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
            ["date_from"] = from.ToString("yyyy-MM-dd"),
            ["date_to"] = to.ToString("yyyy-MM-dd")
        };

        var path = "/flightlog/list/byAircraftFilters";
        var data = await _apiClient.GetAsync<List<FlightLog>>(path,  query, headers, ct);
        return data ?? new List<FlightLog>();
    }


}
