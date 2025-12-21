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
        var token = await _tokenStore.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
