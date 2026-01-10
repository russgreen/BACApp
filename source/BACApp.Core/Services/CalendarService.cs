using BACApp.Core.Abstractions;
using BACApp.Core.Models;
using BACApp.Core.Services;

public class CalendarService : ICalendarService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public CalendarService(IApiClient apiClient,
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<IReadOnlyList<BookingResource>> GetResourcesAsync(DateOnly date, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var path = $"/booking_agenda/ReadResources/{date:yyyy-MM-dd}";
        var data = await _apiClient.GetAsync<List<BookingResource>>(path, null, headers, ct) ?? new List<BookingResource>();
        return data ?? new List<BookingResource>();
    }

    public async Task<IReadOnlyList<BookingEvent>> GetEventsAsync(DateOnly date, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var path = $"/booking_agenda/ReadEvents/{date:yyyy-MM-dd}";
        var data = await _apiClient.GetAsync<List<BookingEvent>>(path, null, headers, ct) ?? new List<BookingEvent>();
        return data ?? new List<BookingEvent>();
    }

    public async Task<BookingAgenda> GetBookingAgendaAsync(int bookingAgndaId, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var path = $"/booking_agenda/{bookingAgndaId}";
        var data = await _apiClient.GetAsync<BookingAgenda>(path, null, headers, ct) ?? new BookingAgenda();
        return data ?? new BookingAgenda();
    }
}
