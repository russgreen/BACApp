using BACApp.Core.Abstractions;
using BACApp.Core.Models;

namespace BACApp.Core.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public InvoiceService(IApiClient apiClient,
        IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task AddInvoiceAsync(Invoice invoice, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Invoice>> GetInvoicesAsync(int userID, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["company-id"] = _authService.UserCompany.CompanyId.ToString()
        };

        var query = new Dictionary<string, string?>
        {
            ["userId"] = userID.ToString(),
            ["dateFrom"] = from.ToString("yyyy-MM-dd"),
            ["dateTo"] = to.ToString("yyyy-MM-dd")
        };

        var path = "/invoice/list/GetInvoicesByFilters";
        var data = await _apiClient.GetAsync<List<Invoice>>(path, query, headers, ct);
        return data ?? new List<Invoice>();
    }
}
