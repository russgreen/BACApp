using BACApp.Core.Services;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI.Avalonia;

internal class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStore _tokenStore;

    public AuthHeaderHandler(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        // Ensure Accept header
        if (!request.Headers.Contains("Accept"))
        {
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
        }

        // Add required API headers: user-token and company-id
        var token = await _tokenStore.GetTokenAsync().ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token))
        {
            if (request.Headers.Contains("Authorization"))
            {
                request.Headers.Remove("Authorization");
            }

            // API requires 'user-token' not 'Authorization'
            if (!request.Headers.Contains("user-token"))
            {
                request.Headers.TryAddWithoutValidation("user-token", token);
            }
        }

        return await base.SendAsync(request, ct);
    }
}
