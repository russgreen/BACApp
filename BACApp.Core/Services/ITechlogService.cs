using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface ITechlogService
{
    Task<IReadOnlyList<TechLog>> GetTechLogsAsync(string aircraftId, DateOnly from, DateOnly to, CancellationToken ct = default);
}
