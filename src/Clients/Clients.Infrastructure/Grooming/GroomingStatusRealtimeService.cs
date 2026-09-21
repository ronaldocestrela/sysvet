using Microsoft.AspNetCore.SignalR.Client;

namespace Clients.Infrastructure.Grooming;

/// <summary>
/// Subscribes to grooming status hub events when online and authenticated.
/// </summary>
public interface IGroomingStatusRealtime : IAsyncDisposable
{
    /// <summary>
    /// Fired when the server pushes a grooming status change for the tenant.
    /// </summary>
    event Func<Task>? StatusChanged;

    /// <summary>
    /// Opens the SignalR connection when connectivity and token are available.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the hub connection.
    /// </summary>
    Task DisconnectAsync();
}

/// <summary>
/// Hub payload mirrored from API <c>GroomingStatusChangedMessage</c>.
/// </summary>
public sealed class GroomingStatusChangedMessage
{
    public Guid GroomingAppointmentId { get; init; }
    public Guid PetId { get; init; }
    public Guid TutorId { get; init; }
    public string Kind { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset OccurredOn { get; init; }
}

/// <summary>
/// SignalR client for the grooming board using the authenticated API HTTP client base address.
/// </summary>
public sealed class GroomingStatusRealtimeService : IGroomingStatusRealtime
{
    private readonly Func<IHttpClientFactory?> _httpClientFactoryAccessor;
    private readonly Func<Task<string?>> _accessTokenProvider;
    private readonly Func<bool> _isOnline;
    private HubConnection? _connection;

    public GroomingStatusRealtimeService(
        Func<IHttpClientFactory?> httpClientFactoryAccessor,
        Func<Task<string?>> accessTokenProvider,
        Func<bool> isOnline)
    {
        _httpClientFactoryAccessor = httpClientFactoryAccessor;
        _accessTokenProvider = accessTokenProvider;
        _isOnline = isOnline;
    }

    public event Func<Task>? StatusChanged;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!_isOnline())
        {
            return;
        }

        var factory = _httpClientFactoryAccessor();
        if (factory is null)
        {
            return;
        }

        var token = await _accessTokenProvider();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var httpClient = factory.CreateClient("API");
        if (httpClient.BaseAddress is null)
        {
            return;
        }

        var hubUri = new Uri(httpClient.BaseAddress, "hubs/grooming-status");
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.AccessTokenProvider = () => _accessTokenProvider();
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<GroomingStatusChangedMessage>("GroomingStatusChanged", async _ =>
        {
            if (StatusChanged is not null)
            {
                await StatusChanged.Invoke();
            }
        });

        await _connection.StartAsync(cancellationToken);
    }

    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync() => await DisconnectAsync();
}
