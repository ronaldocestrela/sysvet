namespace Clients.Infrastructure.Sync;

/// <summary>
/// Shared wake signal so UI/stores can prompt an immediate sync cycle (ADR-026).
/// </summary>
public sealed class SyncWakeSignal
{
    private readonly SemaphoreSlim _wake = new(0, 1);

    /// <summary>Requests the background worker to run a sync cycle soon.</summary>
    public void RequestSync()
    {
        try
        {
            _wake.Release();
        }
        catch (SemaphoreFullException)
        {
            // Already signaled.
        }
    }

    /// <summary>Semaphore awaited by <see cref="SyncBackgroundWorker"/>.</summary>
    internal SemaphoreSlim WakeSemaphore => _wake;
}
