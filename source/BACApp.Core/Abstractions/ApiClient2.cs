using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BACApp.Core.Abstractions;

public class ApiClient2 : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ApiClient2> _logger;

    public ApiClient2(HttpClient httpClient,
        ILogger<ApiClient2> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }

    public async Task<T?> GetAsync<T>(string path, IDictionary<string, string>? headers, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        ApplyHeaders(request, headers);

        var sw = Stopwatch.StartNew();

        // Key change: ResponseHeadersRead + timing markers.
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        var tSend = sw.Elapsed;

        try
        {
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var tStream = sw.Elapsed;

            var result = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, ct).ConfigureAwait(false);
            var tJson = sw.Elapsed;

            System.Diagnostics.Debug.WriteLine(
                $"ApiClient.GetAsync<{typeof(T).Name}> {path} timings: Send={tSend.TotalMilliseconds:F0}ms, Stream={tStream.TotalMilliseconds:F0}ms, Json={tJson.TotalMilliseconds:F0}ms");

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
            "JSON deserialize failed for {Path}",
            path);

            throw;
        }
        finally
        {
            response.Dispose();
        }
    }

    // New: GET with headers (per-request control, e.g., company-id)
    public async Task<T?> GetAsync<T>(string path, IDictionary<string, string?> query, IDictionary<string, string>? headers, CancellationToken ct = default)
    {
        var url = new StringBuilder(path);
        if (query is { Count: > 0 })
        {
            url.Append('?');
            url.Append(string.Join('&',
                query.Where(kv => kv.Value != null)
                     .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url.ToString());
        ApplyHeaders(request, headers);

        var sw = Stopwatch.StartNew();

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        var tSend = sw.Elapsed;

        try
        {
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var tStream = sw.Elapsed;

            var result = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, ct).ConfigureAwait(false);
            var tJson = sw.Elapsed;

            System.Diagnostics.Debug.WriteLine(
                $"ApiClient.GetAsync<{typeof(T).Name}> {url} timings: Send={tSend.TotalMilliseconds:F0}ms, Stream={tStream.TotalMilliseconds:F0}ms, Json={tJson.TotalMilliseconds:F0}ms");

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
            "JSON deserialize failed for {Path}",
            path);

            throw;
        }
        finally
        {
            response.Dispose();
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        using var content = CreateContent(body);
        using var response = await _httpClient.PostAsync(path, content, ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, _jsonOptions, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, IDictionary<string, string>? headers, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = CreateContent(body)
        };

        ApplyHeaders(request, headers);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, _jsonOptions, ct);
    }

    private StringContent CreateContent<TRequest>(TRequest body)
    {
        // If no body is required (e.g., Swagger shows -d ''), send truly empty content.
        if (body is null)
        {
            return new StringContent(string.Empty, Encoding.UTF8, "application/json");
        }

        // Otherwise serialize the request as JSON.
        var json = JsonSerializer.Serialize(body, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static void ApplyHeaders(HttpRequestMessage request, IDictionary<string, string>? headers)
    {
        if (headers is not { Count: > 0 })
            return;

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
}
