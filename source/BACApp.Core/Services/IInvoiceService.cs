using BACApp.Core.Models;

namespace BACApp.Core.Services;

public interface IInvoiceService
{
    Task AddInvoiceAsync(Invoice invoice, CancellationToken ct = default);

    Task<IReadOnlyList<Invoice>> GetInvoicesAsync(int userID, DateOnly from, DateOnly to, CancellationToken ct = default);
}
