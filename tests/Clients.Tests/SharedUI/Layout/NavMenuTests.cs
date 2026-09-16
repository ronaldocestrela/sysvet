using Bunit;
using SharedUI.Layout;
using SharedUI.Navigation;
using Xunit;

namespace Clients.Tests.SharedUI.Layout;

public class NavMenuTests : BunitContext
{
    [Fact]
    public void Should_Render_All_AppNavItems_As_NavLinks()
    {
        var cut = Render<NavMenu>();

        var links = cut.FindAll("a.nav-link");
        Assert.Equal(AppNavItems.All.Count, links.Count);

        foreach (var item in AppNavItems.All)
        {
            Assert.Contains(links, a => a.GetAttribute("href") == item.Href || a.GetAttribute("href") == item.Href.TrimStart('/'));
        }
    }

    [Fact]
    public void Should_Not_Use_Emoji_Icons()
    {
        var cut = Render<NavMenu>();
        Assert.DoesNotContain("👥", cut.Markup);
        Assert.DoesNotContain("🐾", cut.Markup);
    }
}
