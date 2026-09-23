using Bunit;
using Clients.Infrastructure.Platform;
using Clients.Tests.PlatformWeb;
using Core.Domain.Entitlements;
using Microsoft.Extensions.DependencyInjection;
using PlatformWeb.Components;
using PlatformWeb.Pages;
using PlatformWeb.Services;
using SharedUI.Services;

namespace Clients.Tests.PlatformWeb;

public class TenantsPageTests : BunitContext
{
    private readonly FakePlatformAdminApi _api = new();
    private readonly FakePlatformAuthState _auth = new();

    public TenantsPageTests()
    {
        Services.AddSingleton<IPlatformAdminApi>(_api);
        Services.AddSingleton<IPlatformAuthState>(_auth);
        Services.AddSingleton<IAuthState>(_auth);
        Services.AddSingleton<INavigationService, FakeNavigationService>();
    }

    [Fact]
    public void Should_Deny_List_When_Not_SuperAdmin()
    {
        _auth.Roles = ["Admin"];
        var cut = Render<TenantsPage>();
        Assert.Contains("Acesso negado", cut.Markup);
    }

    [Fact]
    public void Should_Render_Tenant_Rows_When_SuperAdmin()
    {
        var cut = Render<TenantsPage>();
        cut.WaitForAssertion(() => Assert.Contains("Clínica A", cut.Markup));
    }

    [Fact]
    public async Task Should_Call_Onboard_When_Form_Submitted()
    {
        var cut = Render<TenantsPage>();
        cut.WaitForAssertion(() => Assert.Contains("Tenants", cut.Find("h1").TextContent));
        var form = cut.Instance.TestOnboardForm;
        form.Slug = "nova-clinica";
        form.DisplayName = "Nova Clínica";
        form.AdminEmail = "admin@nova.com";
        form.AdminPassword = "Password123!";
        form.HeadquartersCnpj = "12345678000199";
        form.HeadquartersLegalName = "Nova Clínica LTDA";
        await cut.InvokeAsync(() => cut.Instance.TestSubmitOnboardAsync());
        Assert.NotNull(_api.LastOnboardRequest);
        Assert.Equal("nova-clinica", _api.LastOnboardRequest!.Slug);
    }
}

public class TenantFeatureFlagsPanelTests : BunitContext
{
    [Fact]
    public async Task Should_Call_SetFeatureFlag_When_Disable_Clicked()
    {
        var api = new FakePlatformAdminApi();
        var tenantId = Guid.NewGuid();
        Services.AddSingleton<IPlatformAdminApi>(api);
        var cut = Render<TenantFeatureFlagsPanel>(parameters => parameters.Add(p => p.TenantId, tenantId));
        cut.FindAll("button")[0].Click();
        cut.WaitForAssertion(() => Assert.NotNull(api.LastFlag));
        Assert.Equal(tenantId, api.LastFlag!.Value.TenantId);
        Assert.Equal(PlatformFeatureFlagState.Disabled, api.LastFlag!.Value.State);
    }
}

public class TenantApiKeysPanelTests : BunitContext
{
    [Fact]
    public async Task Should_Show_Secret_After_Create()
    {
        var api = new FakePlatformAdminApi();
        Services.AddSingleton<IPlatformAdminApi>(api);
        var cut = Render<TenantApiKeysPanel>(parameters => parameters.Add(p => p.TenantId, Guid.NewGuid()));
        cut.Find("input").Change("Contabilidade X");
        cut.Find("button.btn-primary").Click();
        cut.WaitForAssertion(() => cut.Markup.Contains("secret-once-123"));
    }
}

internal sealed class FakeNavigationService : INavigationService
{
    public void NavigateTo(string uri, bool forceLoad = false)
    {
    }
}
