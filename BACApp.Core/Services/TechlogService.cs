using BACApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Services;

public class TechlogService : ITechlogService
{
    private readonly Abstractions.IApiClient _apiClient;

    public TechlogService(Abstractions.IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<TechLog>> GetTechLogsAsync(int companyId, string registration, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = companyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration,
            ["dateFrom"] = from.ToString("yyyy-MM-dd"),
            ["dateTo"] = to.ToString("yyyy-MM-dd")
        };

        var result = await _apiClient.GetAsync<List<TechLog>>("/techlog/list/GetDTLFlightsByAircraft", headers, query, ct);
        return result ?? new List<TechLog>();
    }

    public async Task<MaintenanceData> GetMaintenanceDataAsync(int companyId, string registration, DateOnly date, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = companyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration,
            ["date_from"] = date.ToString("yyyy-MM-dd"),
        };

        var result = await _apiClient.GetAsync<MaintenanceData>("/techlog/maintenance/data", headers, query, ct);
        return result;
    }
}
