namespace BACApp.Core.Abstractions;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, IDictionary<string, string>? headers, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string path, IDictionary<string, string?> query, IDictionary<string, string>? headers, CancellationToken ct = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, IDictionary<string, string>? headers, CancellationToken ct = default);
}
