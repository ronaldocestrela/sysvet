using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SharedUI.Layout;
using Xunit;

namespace Clients.Tests.SharedUI.Layout;

public class AuthLayoutTests : BunitContext
{
    [Fact]
    public void Should_Render_Body_Without_NavMenu()
    {
        ComponentFactories.AddStub<NavMenu>();

        var cut = Render<AuthLayout>(parameters => parameters
            .Add(p => p.Body, (RenderTreeBuilder builder) =>
            {
                builder.AddMarkupContent(0, "<p>Login body</p>");
            }));

        Assert.Contains("Login body", cut.Markup);
        Assert.False(cut.HasComponent<Bunit.TestDoubles.Stub<NavMenu>>());
    }
}
