using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>
/// Typed HTTP wrapper that maps API responses to <see cref="Result{T}"/> for Blazor and MAUI hosts.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a client bound to the host-configured <see cref="HttpClient"/> (typically named "API").
    /// </summary>
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>Downloads a binary file from GET.</summary>
    public async Task<Result<DownloadedFile>> DownloadGetAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await ReadDownloadAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<DownloadedFile>(new Error("ApiClient.Exception", ex.Message));
        }
    }

    /// <summary>Downloads a binary file from POST with JSON body.</summary>
    public async Task<Result<DownloadedFile>> DownloadPostAsync<TRequest>(string url, TRequest body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: _jsonOptions)
            };
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ReadDownloadAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<DownloadedFile>(new Error("ApiClient.Exception", ex.Message));
        }
    }

    /// <summary>Performs an HTTP GET and deserializes the JSON body on success.</summary>
    public async Task<Result<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return await HandleResponseAsync<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(new Error("ApiClient.Exception", ex.Message));
        }
    }

    /// <summary>Performs an HTTP POST with JSON body; supports 200/201 responses with a body.</summary>
    public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest body,
        Guid? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        SendJsonAsync<TRequest, TResponse>(HttpMethod.Post, url, body, idempotencyKey, cancellationToken);

    /// <summary>Performs an HTTP POST with JSON body; treats 204 No Content as success without payload.</summary>
    public Task<Result> PostAsync<TRequest>(
        string url,
        TRequest body,
        Guid? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        SendJsonNoContentAsync(HttpMethod.Post, url, body, idempotencyKey, cancellationToken);

    /// <summary>Performs an HTTP PUT with JSON body; treats 204 No Content as success.</summary>
    public Task<Result> PutAsync<TRequest>(
        string url,
        TRequest body,
        Guid? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        SendJsonNoContentAsync(HttpMethod.Put, url, body, idempotencyKey, cancellationToken);

    /// <summary>Performs an HTTP PUT with JSON body; supports 200 responses with a body.</summary>
    public Task<Result<TResponse>> PutAsync<TRequest, TResponse>(
        string url,
        TRequest body,
        Guid? idempotencyKey = null,
        CancellationToken cancellationToken = default) =>
        SendJsonAsync<TRequest, TResponse>(HttpMethod.Put, url, body, idempotencyKey, cancellationToken);

    /// <summary>Performs an HTTP DELETE; treats 204 No Content as success.</summary>
    public async Task<Result> DeleteAsync(
        string url,
        Guid? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            ApplyIdempotencyKey(request, idempotencyKey);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleNoContentAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ApiClient.Exception", ex.Message));
        }
    }

    private async Task<Result<TResponse>> SendJsonAsync<TRequest, TResponse>(
        HttpMethod method,
        string url,
        TRequest body,
        Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body, options: _jsonOptions)
            };
            ApplyIdempotencyKey(request, idempotencyKey);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.NoContent)
            {
                return Result.Failure<TResponse>(new Error("ApiClient.UnexpectedNoContent", "Expected a response body."));
            }

            return await HandleResponseAsync<TResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<TResponse>(new Error("ApiClient.Exception", ex.Message));
        }
    }

    private async Task<Result> SendJsonNoContentAsync<TRequest>(
        HttpMethod method,
        string url,
        TRequest body,
        Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body, options: _jsonOptions)
            };
            ApplyIdempotencyKey(request, idempotencyKey);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await HandleNoContentAsync(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ApiClient.Exception", ex.Message));
        }
    }

    private static void ApplyIdempotencyKey(HttpRequestMessage request, Guid? idempotencyKey)
    {
        if (idempotencyKey is { } key && key != Guid.Empty)
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", key.ToString());
        }
    }

    private async Task<Result> HandleNoContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return Result.Success();
        }

        return await MapFailureAsync(response, cancellationToken);
    }

    private async Task<Result<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
            return data is not null
                ? Result.Success(data)
                : Result.Failure<T>(new Error("ApiClient.NullResponse", "Response content was null"));
        }

        return await MapFailureAsync<T>(response, cancellationToken);
    }

    private async Task<Result<T>> MapFailureAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var failure = await MapFailureAsync(response, cancellationToken);
        return Result.Failure<T>(failure.Error);
    }

    private async Task<Result<DownloadedFile>> ReadDownloadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var failure = await MapFailureAsync(response, cancellationToken);
            return Result.Failure<DownloadedFile>(failure.Error);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "download.bin";
        return Result.Success(new DownloadedFile(bytes, contentType, fileName));
    }

    private async Task<Result> MapFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var problem = await response.Content.ReadFromJsonAsync<HttpProblemDetails>(_jsonOptions, cancellationToken);

        if (problem is not null)
        {
            var errorCode = problem.Status?.ToString() ?? "Unknown";
            var errorMessage = !string.IsNullOrWhiteSpace(problem.Title)
                ? $"{problem.Title}: {problem.Detail}"
                : "Unknown error occurred.";

            return Result.Failure(new Error(errorCode, errorMessage));
        }

        return Result.Failure(new Error(response.StatusCode.ToString(), "An error occurred but no problem details were provided."));
    }
}
