using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface ITechlogService
{
    Task<IReadOnlyList<TechLog>> GetTechLogsAsync(string registration, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<MaintenanceData> GetMaintenanceDataAsync(string registration, DateOnly dateFrom, CancellationToken ct = default);
}
