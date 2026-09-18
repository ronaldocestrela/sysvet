using Core.Domain;

namespace Core.Application.Sync;

/// <summary>
/// Maps module-specific outbox payloads to MediatR commands during sync push.
/// </summary>
public interface ISyncPushHandler
{
    /// <summary>Attempts to map a sync message to a command; returns null when unsupported.</summary>
    object? TryMapCommand(SyncOutboxMessageDto message);

    /// <summary>Dispatches a command previously produced by <see cref="TryMapCommand"/>.</summary>
    Task<Result> DispatchAsync(object command, CancellationToken cancellationToken);
}
