using Bunit;
using Microsoft.AspNetCore.Components;
using SharedUI.Components;
using Xunit;

namespace Clients.Tests.SharedUI.Components;

public class ModalTests : BunitContext
{
    [Fact]
    public void Should_Not_Render_When_IsVisible_Is_False()
    {
        var cut = Render<Modal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Title, "Test"));

        Assert.DoesNotContain("modal-backdrop", cut.Markup);
    }

    [Fact]
    public void Should_Render_Title_And_Dialog_Semantics_When_Visible()
    {
        var cut = Render<Modal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Confirmar"));

        cut.Find("[role=\"dialog\"]");
        var dialog = cut.Find("[aria-modal=\"true\"]");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        cut.Find("h3").TextContent.MarkupMatches("Confirmar");
        cut.Find("button.close-btn[aria-label=\"Fechar\"]");
    }

    [Fact]
    public void Should_Invoke_IsVisibleChanged_When_Close_Clicked()
    {
        var visible = true;
        var cut = Render<Modal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.IsVisibleChanged, EventCallback.Factory.Create<bool>(this, v => visible = v))
            .Add(p => p.Title, "T"));

        cut.Find("button.close-btn").Click();
        Assert.False(visible);
    }
}
