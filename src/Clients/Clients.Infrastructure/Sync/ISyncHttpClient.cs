using System.Net.Http.Json;
using Clients.Infrastructure.Sync;
using System.Text.Json;

namespace Clients.Infrastructure.Sync;

public interface ISyncHttpClient
{
    Task<bool> PushAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken);
    // Pull methods will be added later
}

public class SyncHttpClient : ISyncHttpClient
{
    private readonly HttpClient _httpClient;

    public SyncHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> PushAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken)
    {
        if (!messages.Any()) return true;

        var response = await _httpClient.PostAsJsonAsync("/api/v1/sync/push", messages, cancellationToken);
        
        // Return true if success, false if server rejected (e.g. 409 Conflict, 400 Bad Request)
        // More granular error handling will be done in production.
        return response.IsSuccessStatusCode;
    }
}
