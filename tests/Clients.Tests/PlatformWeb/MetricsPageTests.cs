using Bunit;
using Clients.Infrastructure.Platform;
using Clients.Tests.PlatformWeb;
using Microsoft.Extensions.DependencyInjection;
using PlatformWeb.Pages;
using PlatformWeb.Services;
using SharedUI.Services;

namespace Clients.Tests.PlatformWeb;

public class MetricsPageTests : BunitContext
{
    private readonly FakePlatformAdminApi _api = new();
    private readonly FakePlatformAuthState _auth = new();

    public MetricsPageTests()
    {
        Services.AddSingleton<IPlatformAdminApi>(_api);
        Services.AddSingleton<IPlatformAuthState>(_auth);
        Services.AddSingleton<IAuthState>(_auth);
        Services.AddSingleton<INavigationService, FakeNavigationService>();
    }

    [Fact]
    public void Should_Deny_When_Not_SuperAdmin()
    {
        _auth.Roles = ["Admin"];
        var cut = Render<MetricsPage>();
        Assert.Contains("Acesso negado", cut.Markup);
    }

    [Fact]
    public void Should_Render_Mrr_When_SuperAdmin()
    {
        var cut = Render<MetricsPage>();
        cut.WaitForAssertion(() => Assert.Contains("MRR faturado", cut.Markup));
        cut.WaitForAssertion(() => Assert.Contains("600", cut.Markup));
    }
}
