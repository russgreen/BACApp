using System.Text;
using BACApp.Core.Abstractions;
using BACApp.Core.DTO;
using BACApp.Core.Messages;
using CommunityToolkit.Mvvm.Messaging;

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

        WeakReferenceMessenger.Default.Register<SelectedCompanyMessage>(this, (r, m) =>
        {
            UserCompany = m.Value;

            WeakReferenceMessenger.Default.Send(new LoggedInMessage(true));
        });
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

        //only allow logins to Bristol Aeroclub or the Cloudbase Demo club
        var allowedCompanyIds = new HashSet<int> { 1002, 1158 };

        // If only one company is returned and it's not allowed, fail login.
        if (result.Companies.Count() == 1 && !allowedCompanyIds.Contains(result.Companies[0].CompanyId))
        {
            CurrentLogin = null;
            return null;
        }

        // If more than one company is returned, remove any non-allowed companies.
        if (result.Companies.Count() > 1)
        {
            var filteredCompanies = result.Companies
                .Where(c => allowedCompanyIds.Contains(c.CompanyId))
                .ToArray();

            result.Companies = filteredCompanies;

            // If filtering removed everything, fail login.
            if (result.Companies.Count() == 0)
            {
                CurrentLogin = null;
                return null;
            }
        }

        await _tokenStore.SetTokenAsync(result.Token);

        CurrentLogin = result;

        return result;
    }

    public async Task LogoutAsync()
    {
        CurrentLogin = null;
        await _tokenStore.ClearAsync();
    }

    public Task<string?> GetTokenAsync() => _tokenStore.GetTokenAsync();
}
