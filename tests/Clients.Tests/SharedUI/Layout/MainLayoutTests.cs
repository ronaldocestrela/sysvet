using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Layout;
using Xunit;
using SharedUI.Services;
using Clients.Tests.Fakes;

namespace Clients.Tests.SharedUI.Layout;

public class MainLayoutTests : BunitContext
{
    public MainLayoutTests()
    {
        Services.AddSingleton<IToastService, ToastService>();
        Services.AddScoped<IAuthState, FakeAuthState>();
        Services.AddScoped<IConnectivityService, FakeConnectivityService>();
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
}
