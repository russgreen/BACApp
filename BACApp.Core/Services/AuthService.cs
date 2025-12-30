using System.Text;
using BACApp.Core.Abstractions;
using BACApp.Core.DTO;

namespace BACApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;
    private readonly ITokenStore _tokenStore;

    public LoginResponse? CurrentLogin { get; private set; }

    public CompanyDto UserCompany { get; private set; }

    public AuthService(IApiClient apiClient, ITokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public async Task<LoginResponse> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default)
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
                ["accept"] = "application/json"
            },
            ct);

        if (result is null || string.IsNullOrWhiteSpace(result.Token))
        {
            CurrentLogin = null;
            return null;
        }

        await _tokenStore.SetTokenAsync(result.Token);

        CurrentLogin = result;
        UserCompany = CurrentLogin.Companies.First(); //TODO handle multi-company logins

        return result;
    }

    public async Task LogoutAsync()
    {
        CurrentLogin = null;
        await _tokenStore.ClearAsync();
    }

    public Task<string?> GetTokenAsync() => _tokenStore.GetTokenAsync();
}
