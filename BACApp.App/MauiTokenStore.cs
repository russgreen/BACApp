using BACApp.Core.Services;

namespace BACApp.App;

public class MauiTokenStore : ITokenStore
{
    private const string Key = "auth_token";

    public async Task SetTokenAsync(string token)
    {
        await SecureStorage.SetAsync(Key, token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try { return await SecureStorage.GetAsync(Key); }
        catch { return null; }
    }

    public Task ClearAsync()
    {
        SecureStorage.Remove(Key);
        return Task.CompletedTask;
    }
}
