using BACApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Services;

public class TechlogService : ITechlogService
{
    private readonly Abstractions.IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly IAircraftService _aircraftService;

    public TechlogService(Abstractions.IApiClient apiClient,
        IAuthService authService,
        IAircraftService aircraftService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _aircraftService = aircraftService;
    }

    public async Task<IReadOnlyList<TechLog>> GetAllTechLogsAsync(CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var techlogTasks = _aircraftService.AllCompanyAircraft
            .Select(async aircraft =>
            {
                var query = new Dictionary<string, string?>
                {
                    ["registration"] = aircraft.Registration
                };

                var path = "/techlog/list/GetDTLFlightsByFilters";
                return await _apiClient.GetAsync<List<TechLog>>(path, query, headers, ct) ?? new List<TechLog>();
            })
            .ToList();

        var logsByAircraft = await Task.WhenAll(techlogTasks);
        return logsByAircraft.SelectMany(x => x).ToList();
    }

    public async Task<IReadOnlyList<TechLog>> GetTechLogsAsync(string registration, DateOnly from, DateOnly to, CancellationToken ct = default)
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

        var path = "/techlog/list/GetDTLFlightsByFilters";
        var result = await _apiClient.GetAsync<List<TechLog>>(path, query, headers, ct);
        return result ?? new List<TechLog>();
    }

    public async Task<MaintenanceData> GetMaintenanceDataAsync(string registration, DateOnly date, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["registration"] = registration,
            ["dateFrom"] = date.ToString("yyyy-MM-dd"),
        };

        var path = "/techlog/maintenance/data";
        var result = await _apiClient.GetAsync<MaintenanceData>(path, query, headers, ct);
        return result;
    }


}
