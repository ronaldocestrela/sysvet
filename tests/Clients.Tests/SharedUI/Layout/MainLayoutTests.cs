using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Layout;
using Xunit;
using SharedUI.Services;

namespace Clients.Tests.SharedUI.Layout;

public class MainLayoutTests : BunitContext
{
    public MainLayoutTests()
    {
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddScoped<IAuthState, DummyAuthState>();
        Services.AddScoped<IConnectivityService, DummyConnectivityService>();
    }

    [Fact]
    public void Should_Render_MainLayout_Structure_And_Body()
    {
        // Arrange
        ComponentFactories.AddStub<NavMenu>();

        // Act
        var cut = Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderTreeBuilder builder) =>
            {
                builder.AddMarkupContent(0, "<h1>Conteúdo da Página</h1>");
            })
        );

        // Assert
        cut.Find("h2").TextContent.MarkupMatches("SysVet | VetNexus");
        cut.Find(".content-body").MarkupMatches(@"
            <div class=""content-body"">
                <h1>Conteúdo da Página</h1>
            </div>
        ");
        
        // Assert NavMenu stub is rendered
        Assert.True(cut.HasComponent<Bunit.TestDoubles.Stub<NavMenu>>());
        cut.Find(".toast-container");
    }

    private class DummyAuthState : IAuthState
    {
        public bool IsAuthenticated => true;
        public IReadOnlyList<string> Menus { get; } = [];
        public event EventHandler? SessionChanged;

        public Task InitializeAsync() => Task.CompletedTask;
        public Task<string?> GetTokenAsync() => Task.FromResult<string?>("dummy-token");
        public Task<string?> GetRefreshTokenAsync() => Task.FromResult<string?>("refresh");
        public Task LoginAsync(string accessToken, string refreshToken) => Task.CompletedTask;
        public Task SetMenusAsync(IReadOnlyList<string> menus) => Task.CompletedTask;
        public Task LogoutAsync() => Task.CompletedTask;
    }

    private class DummyConnectivityService : IConnectivityService
    {
        public ConnectivityStatus Status => ConnectivityStatus.Online;
        public bool IsOnline => true;
        public event EventHandler<ConnectivityStatus>? StatusChanged;
        public void SetSyncing(bool isSyncing) { }
    }
}
