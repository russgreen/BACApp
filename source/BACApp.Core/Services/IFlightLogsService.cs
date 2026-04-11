using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface IFlightLogsService
{
    Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(string registration, CancellationToken ct = default);
    Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(string registration, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<FlightLog>> GetAllFlightLogsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FlightLog>> GetFlightLogsUnbilledAsync(CancellationToken ct = default);
}
