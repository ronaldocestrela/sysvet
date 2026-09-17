using SharedUI.Services;

namespace Clients.Tests.Fakes;

/// <summary>
/// Test double for <see cref="IConnectivityService"/> that raises <see cref="StatusChanged"/> on mutations.
/// </summary>
public sealed class FakeConnectivityService : IConnectivityService
{
    public FakeConnectivityService(ConnectivityStatus initialStatus = ConnectivityStatus.Online)
    {
        Status = initialStatus;
    }

    public ConnectivityStatus Status { get; private set; }

    public bool IsOnline => Status == ConnectivityStatus.Online;

    public int SyncingCalls { get; private set; }

    public event EventHandler<ConnectivityStatus>? StatusChanged;

    public void SetSyncing(bool isSyncing)
    {
        SyncingCalls++;
        Status = isSyncing ? ConnectivityStatus.Syncing : ConnectivityStatus.Online;
        StatusChanged?.Invoke(this, Status);
    }

    public void RaiseOnline()
    {
        Status = ConnectivityStatus.Online;
        StatusChanged?.Invoke(this, Status);
    }
}
