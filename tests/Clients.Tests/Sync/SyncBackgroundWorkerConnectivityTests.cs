using Clients.Infrastructure.Sync;
using FluentAssertions;
using SharedUI.Services;
using Xunit;
using Clients.Tests.Fakes;

namespace Clients.Tests.Sync;

public class SyncConnectivityAdapterTests
{
    [Fact]
    public void SetSyncing_ShouldForwardToConnectivityService()
    {
        var connectivity = new FakeConnectivityService();
        var adapter = new SyncConnectivityAdapter(connectivity);

        adapter.SetSyncing(true);

        connectivity.SyncingCalls.Should().Be(1);
    }

    [Fact]
    public void OnlineTransition_ShouldRaiseOnlineStateChanged()
    {
        var connectivity = new FakeConnectivityService(ConnectivityStatus.Offline);
        var adapter = new SyncConnectivityAdapter(connectivity);
        var raised = false;
        adapter.OnlineStateChanged += (_, _) => raised = true;

        connectivity.RaiseOnline();

        raised.Should().BeTrue();
    }
}
