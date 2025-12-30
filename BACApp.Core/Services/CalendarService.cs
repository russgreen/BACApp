namespace BACApp.Core.Services;

public class CalendarService : ICalendarService
{
    private readonly Abstractions.IApiClient _apiClient;

    public CalendarService(Abstractions.IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<object>> GetResourcesAsync(DateOnly date, CancellationToken ct = default)
    {
        var path = $"/booking_agenda/ReadResources/{date:yyyy-MM-dd}";
        var data = await _apiClient.GetAsync<List<object>>(path, null, ct) ?? new List<object>();
        return data;
    }

    public async Task<IReadOnlyList<object>> GetEventsAsync(DateOnly date, CancellationToken ct = default)
    {
        var path = $"/booking_agenda/ReadEvents/{date:yyyy-MM-dd}";
        var data = await _apiClient.GetAsync<List<object>>(path, null, ct) ?? new List<object>();
        return data;
    }
}
