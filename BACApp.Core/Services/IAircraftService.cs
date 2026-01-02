using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface IAircraftService
{
    /// <summary>
    /// The full list of aircraft for the user's company. Populated on service initialization.
    /// </summary>
    List<Aircraft> AllCompanyAircraft { get;}

    /// <summary>
    /// Refresh the full list of aircraft for the user's company. Only needed if company context changes or aircraft are added or removed at runtime.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task LoadAllCompanyAircraftAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a list of aircraft for the specified company.
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="ct"></param>
    /// <returns>The full list of aircraft for the specified company.</returns>
    Task<IReadOnlyList<Models.Aircraft>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default);
}
