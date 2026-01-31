using BACApp.Core.Services;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI;

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

        // Add required API headers: user-token
        var token = await _tokenStore.GetTokenAsync().ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token))
        {
            //we have a token so we've already authorised.
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

        try
        {
            return await base.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            // Expected when navigating away / disposing the page and canceling in-flight requests.
            // Keep cancellation semantics intact (do not wrap; do not log here).
            throw;
        }
        catch (HttpRequestException ex) when (ct.IsCancellationRequested && ex.InnerException is IOException)
        {
            // Some socket aborts during cancellation surface as HttpRequestException/IOException.
            throw new TaskCanceledException("The operation was canceled.", ex, ct);
        }
    }
}
