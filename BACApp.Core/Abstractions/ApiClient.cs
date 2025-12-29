using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BACApp.Core.Abstractions;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string path, IDictionary<string, string?>? query = null, CancellationToken ct = default)
    {
        var url = new StringBuilder(path);
        if (query is { Count: > 0 })
        {
            url.Append('?');
            url.Append(string.Join('&', query.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")));
        }

        using var response = await _httpClient.GetAsync(url.ToString(), ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        using var content = CreateContent(body);
        using var response = await _httpClient.PostAsync(path, content, ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, IDictionary<string, string>? headers, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = CreateContent(body)
        };

        if (headers is { Count: > 0 })
        {
            foreach (var kv in headers)
            {
                if (string.Equals(kv.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Authorization = AuthenticationHeaderValue.Parse(kv.Value);
                }
                else if (string.Equals(kv.Key, "Accept", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.ParseAdd(kv.Value);
                }
                else
                {
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
        }

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
    }

    private static StringContent CreateContent<TRequest>(TRequest body)
    {
        // If no body is required (e.g., Swagger shows -d ''), send truly empty content.
        if (body is null)
        {
            return new StringContent(string.Empty, Encoding.UTF8, "application/json");
        }

        // Otherwise serialize the request as JSON.
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
