namespace BACApp.Core.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
}
