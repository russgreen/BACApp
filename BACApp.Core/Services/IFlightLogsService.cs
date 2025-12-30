using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface IFlightLogsService
{
    Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(int companyId, string registration, CancellationToken ct = default);
    Task<IReadOnlyList<FlightLog>> GetFlightLogsAsync(int companyId, string registration, DateOnly from, DateOnly to, CancellationToken ct = default);
   
}
