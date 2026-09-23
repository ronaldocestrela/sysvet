using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Navigation;
using SharedUI.Routing;
using SharedUI.Services;
using Xunit;
using Clients.Infrastructure.Billing;
using Clients.Tests.Fakes;

namespace Clients.Tests.SharedUI.Routing;

public class AuthorizeRouteViewTests : BunitContext
{
    [Fact]
    public void Unauthenticated_User_Redirects_To_Login()
    {
        var nav = new FakeNavigationService();
        Services.AddSingleton<IAuthState>(new ClientAuthState(new InMemoryTokenStorage()));
        Services.AddSingleton<INavigationService>(nav);

        var routeData = new RouteData(typeof(global::SharedUI.Pages.Tutors), new Dictionary<string, object?>());

        Render<AuthorizeRouteView>(p => p.Add(x => x.RouteData, routeData));

        Assert.Equal(AppRoutes.Login, nav.LastTarget);
    }

    [Fact]
    public async Task Authenticated_User_On_Protected_Route_Does_Not_Redirect_To_Login()
    {
        var storage = new InMemoryTokenStorage();
        await storage.SetTokensAsync("jwt", "refresh");
        var authState = new ClientAuthState(storage);
        await authState.InitializeAsync();
        var nav = new FakeNavigationService();

        Services.AddSingleton<IAuthState>(authState);
        Services.AddSingleton<INavigationService>(nav);
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddScoped<IConnectivityService>(_ => new FakeConnectivityService());
        Services.AddSingleton<IClinicBillingApi, FakeClinicBillingApi>();
        ComponentFactories.AddStub<global::SharedUI.Layout.NavMenu>();
        ComponentFactories.AddStub<global::SharedUI.Pages.Tutors>();

        var routeData = new RouteData(typeof(global::SharedUI.Pages.Tutors), new Dictionary<string, object?>());

        Render<AuthorizeRouteView>(p => p.Add(x => x.RouteData, routeData));

        Assert.Null(nav.LastTarget);
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public string? LastTarget { get; private set; }

        public void NavigateTo(string uri, bool forceLoad = false) => LastTarget = uri;
    }
}
