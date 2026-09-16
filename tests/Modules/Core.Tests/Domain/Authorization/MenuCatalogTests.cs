using Core.Domain.Authorization;
using FluentAssertions;

namespace Core.Tests.Domain.Authorization;

public class MenuCatalogTests
{
    [Fact]
    public void ResolveMenus_ShouldExcludeTutors_WhenCashierPermissions()
    {
        var menus = MenuCatalog.ResolveMenus(Permissions.CashierDefaults());

        menus.Should().NotContain("tutors");
        menus.Should().Contain("sales");
    }
}
