using System.Net.Http.Json;
using Clients.Infrastructure.Sync;

namespace Clients.Infrastructure.Sync;

/// <summary>
/// HTTP client for sync push/pull endpoints (ADR-002).
/// </summary>
public interface ISyncHttpClient
{
    /// <summary>Pushes pending outbox messages to the API.</summary>
    Task<ClientSyncPushResult?> PushAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken);

    /// <summary>Pulls CRM changes since the given cursor.</summary>
    Task<ClientPullChangesResult?> PullAsync(DateTimeOffset since, int take, CancellationToken cancellationToken);
}

/// <inheritdoc />
public class SyncHttpClient : ISyncHttpClient
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates the client.</summary>
    public SyncHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<ClientSyncPushResult?> PushAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken)
    {
        var list = messages.ToList();
        if (list.Count == 0)
        {
            return new ClientSyncPushResult();
        }

        var dto = list.Select(m => new
        {
            m.Id,
            m.Type,
            m.Payload,
            m.CreatedAt
        });

        var response = await _httpClient.PostAsJsonAsync("/api/v1/sync/push", dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ClientSyncPushResult>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ClientPullChangesResult?> PullAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var url = $"/api/v1/sync/pull?since={Uri.EscapeDataString(since.ToString("O"))}&take={take}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ClientPullChangesResult>(cancellationToken);
    }
}
