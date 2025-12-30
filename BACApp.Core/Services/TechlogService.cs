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
