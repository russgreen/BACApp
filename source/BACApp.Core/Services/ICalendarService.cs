using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface ICalendarService
{
    /// <summary>
    /// Get Calendar Resources
    /// </summary>
    /// <param name="date"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IReadOnlyList<BookingResource>> GetResourcesAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Get Calendar Events
    /// </summary>
    /// <param name="date"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IReadOnlyList<BookingEvent>> GetEventsAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Get Booking Agenda
    /// </summary>
    /// <param name="bookingAgndaId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<BookingAgenda> GetBookingAgendaAsync(int bookingAgndaId, CancellationToken ct = default);
}
