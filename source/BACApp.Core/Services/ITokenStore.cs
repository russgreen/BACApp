namespace BACApp.Core.Services;

public interface ITokenStore
{
    Task SetTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task ClearAsync();
}
