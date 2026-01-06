using BACApp.Core.Abstractions;
using BACApp.Core.Models;

namespace BACApp.Core.Services;

public class AircraftService : IAircraftService
{
    private readonly IApiClient _apiClient;    
    private readonly IAuthService _authService;

    public List<Aircraft> AllCompanyAircraft { get; private set; }

    public AircraftService(IApiClient apiClient,
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        LoadAllCompanyAircraftAsync().ConfigureAwait(false);
    }

    public async Task LoadAllCompanyAircraftAsync(CancellationToken ct = default)
    {
        var aircraft = await GetByCompanyIdAsync(ct);
        AllCompanyAircraft = aircraft?.ToList() ?? new List<Aircraft>();
    }

    public Task<IReadOnlyList<Models.Aircraft>?> GetByCompanyIdAsync(CancellationToken ct = default)
    {
        // API expects company-id as a header, not as a query parameter.
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        return _apiClient.GetAsync<IReadOnlyList<Models.Aircraft>>("/aircraft/list/GetAircraftCompanyId", headers, ct);
    }
}
