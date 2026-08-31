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
        // Stub services that might be required by NavMenu or Layout
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
    }

    private class DummyAuthState : IAuthState
    {
        public bool IsAuthenticated => true;
        public string? UserName => "Test User";

        public Task<string?> GetTokenAsync() => Task.FromResult<string?>("dummy-token");
        public Task LoginAsync(string token) => Task.CompletedTask;
        public Task LogoutAsync() => Task.CompletedTask;
    }

    private class DummyConnectivityService : IConnectivityService
    {
        public bool IsOnline => true;
        public event EventHandler<bool>? ConnectivityChanged;
    }
}
