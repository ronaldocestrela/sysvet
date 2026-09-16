using SharedUI.Navigation;
using Xunit;

namespace Clients.Tests.SharedUI.Navigation;

public class MenuNavigationTests
{
    [Fact]
    public void BuildNavItems_Always_Includes_Dashboard()
    {
        var items = MenuNavigation.BuildNavItems(["tutors", "pets"]);

        Assert.Equal("/", items[0].Href);
        Assert.Contains(items, i => i.Href == AppRoutes.Tutors);
        Assert.Contains(items, i => i.Href == AppRoutes.Pets);
    }

    [Fact]
    public void BuildNavItems_When_Empty_Falls_Back_To_Static_List()
    {
        var items = MenuNavigation.BuildNavItems([]);
        Assert.Equal(AppNavItems.All.Count, items.Count);
    }
}
