using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface ICalendarService
{
    Task<IReadOnlyList<object>> GetResourcesAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetEventsAsync(DateOnly date, CancellationToken ct = default);
}
