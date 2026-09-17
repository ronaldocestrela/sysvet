namespace Clients.Infrastructure.Sync;

/// <summary>
/// Single-row cursor for the last successful pull watermark (client-only).
/// </summary>
public sealed class SyncState
{
    /// <summary>Fixed singleton key.</summary>
    public int Id { get; set; } = 1;

    /// <summary>Exclusive lower bound for the next pull (<c>since</c> parameter).</summary>
    public DateTimeOffset LastPullAt { get; set; } = DateTimeOffset.MinValue;
}
