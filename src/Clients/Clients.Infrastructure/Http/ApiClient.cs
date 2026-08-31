using System.Net.Http.Json;
using System.Text.Json;
using Core.Domain;

namespace Clients.Infrastructure.Http;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

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

    private async Task<Result<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
            return data is not null
                ? Result.Success<T>(data)
                : Result.Failure<T>(new Error("ApiClient.NullResponse", "Response content was null"));
        }

        var problem = await response.Content.ReadFromJsonAsync<HttpProblemDetails>(_jsonOptions, cancellationToken);
        
        if (problem is not null)
        {
            var errorCode = problem.Status?.ToString() ?? "Unknown";
            var errorMessage = !string.IsNullOrWhiteSpace(problem.Title) 
                ? $"{problem.Title}: {problem.Detail}" 
                : "Unknown error occurred.";
                
            return Result.Failure<T>(new Error(errorCode, errorMessage));
        }

        return Result.Failure<T>(new Error(response.StatusCode.ToString(), "An error occurred but no problem details were provided."));
    }
}
