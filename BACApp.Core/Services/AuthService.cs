using System.Text;
using BACApp.Core.Abstractions;

namespace BACApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly ITokenStore _tokenStore;

    public AuthService(IApiClient apiClient, ITokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public async Task<bool> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default)
    {
        // Build Basic auth header: base64(username:password)
        var credentials = $"{usernameOrEmail}:{password}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        var authorizationValue = $"Basic {base64Credentials}";

        // Request authentication; expect strongly-typed JSON response.
        var result = await _apiClient.PostAsync<object?, LoginResponse>(
            "/user/authenticate",
            null,
            new Dictionary<string, string>
            {
                ["Authorization"] = authorizationValue,
                ["Accept"] = "application/json"
            },
            ct);

        if (result is null || string.IsNullOrWhiteSpace(result.Token))
            return false;

        await _tokenStore.SetTokenAsync(result.Token);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearAsync();
    }

    public Task<string?> GetTokenAsync() => _tokenStore.GetTokenAsync();
}

public sealed class LoginResponse
{
    public string? First_Name { get; set; } // if API uses snake_case
    public string? Last_Name { get; set; }
    public int? User_Id { get; set; }
    public string? Email { get; set; }

    // The token field per API docs
    public string? Token { get; set; }

    public string? Avatar { get; set; }

    public CompanyDto[]? Companies { get; set; }
}

public sealed class CompanyDto
{
    public string? Company_Name { get; set; }
    public int? Company_Id { get; set; }
    public string? Logo { get; set; }
}
