using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Services;

public class AircraftService : IAircraftService
{
    private readonly Abstractions.IApiClient _apiClient;

    public AircraftService(Abstractions.IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<IReadOnlyList<Models.Aircraft>?> GetByCompanyIdAsync(int companyId, CancellationToken ct = default)
    {
        // API expects company-id as a header, not as a query parameter.
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = companyId.ToString()
        };

        return _apiClient.GetAsync<IReadOnlyList<Models.Aircraft>>("/aircraft/list/GetAircraftCompanyId", headers, ct);
    }
}
