using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Services;

public interface IAircraftService
{
    Task<IReadOnlyList<Models.Aircraft>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default);
}
