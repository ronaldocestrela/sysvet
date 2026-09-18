using Clients.Infrastructure.Sync;

namespace API.IntegrationTests.Sync;

/// <summary>
/// Test double for <see cref="ISyncConnectivity"/> with explicit online toggling.
/// </summary>
public sealed class FakeSyncConnectivity : ISyncConnectivity
{
    private bool _isOnline;

    /// <inheritdoc />
    public bool IsOnline => _isOnline;

    /// <inheritdoc />
    public event EventHandler? OnlineStateChanged;

    /// <summary>Marks the client as online and raises <see cref="OnlineStateChanged"/> when transitioning from offline.</summary>
    public void SetOnline(bool isOnline)
    {
        var wasOnline = _isOnline;
        _isOnline = isOnline;
        if (isOnline && !wasOnline)
        {
            OnlineStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public void SetSyncing(bool isSyncing)
    {
        // No-op for PoC harness; UI adapter is not under test here.
    }
}
