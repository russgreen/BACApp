using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface ITechlogService
{
    Task<IReadOnlyList<TechLog>> GetTechLogsAsync(int companyId, string registration, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<MaintenanceData> GetMaintenanceDataAsync(int companyId, string registration, DateOnly dateFrom, CancellationToken ct = default);
}
