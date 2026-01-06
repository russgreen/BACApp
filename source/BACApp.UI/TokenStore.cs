using BACApp.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI;

internal class TokenStore : ITokenStore
{
    private const string Key = "auth_token";
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static string? _token;

    public async Task SetTokenAsync(string token)
    {
        if (token is null) throw new ArgumentNullException(nameof(token));

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            _token = token;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var value = _token;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            _token = null;
        }
        finally
        {
            _lock.Release();
        }
    }
}
