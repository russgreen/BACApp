using BACApp.Core.DTO;

namespace BACApp.Core.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();

    // Expose current session details
    LoginResponse? CurrentLogin { get; }

    CompanyDto UserCompany { get; }
}
