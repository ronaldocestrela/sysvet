using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Layout;
using SharedUI.Navigation;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Layout;

public class NavMenuTests : BunitContext
{
    public NavMenuTests()
    {
        Services.AddSingleton<IAuthState, DummyAuthState>();
    }

    [Fact]
    public void Should_Render_All_AppNavItems_As_NavLinks_When_No_Menus()
    {
        var cut = Render<NavMenu>();

        var links = cut.FindAll("a.nav-link");
        Assert.Equal(AppNavItems.All.Count, links.Count);
    }

    [Fact]
    public void Should_Filter_By_Menus_From_AuthState()
    {
        Services.AddSingleton<IAuthState>(new DummyAuthState(["tutors", "pets"]));
        var cut = Render<NavMenu>();

        var links = cut.FindAll("a.nav-link");
        Assert.Equal(3, links.Count);
        Assert.Contains(links, a => a.TextContent.Contains("Tutores"));
        Assert.Contains(links, a => a.TextContent.Contains("Pets"));
    }

    [Fact]
    public void Should_Not_Use_Emoji_Icons()
    {
        var cut = Render<NavMenu>();
        Assert.DoesNotContain("👥", cut.Markup);
        Assert.DoesNotContain("🐾", cut.Markup);
    }

    private sealed class DummyAuthState : IAuthState
    {
        private readonly IReadOnlyList<string> _menus;

        public DummyAuthState(IReadOnlyList<string>? menus = null)
        {
            _menus = menus ?? [];
        }

        public bool IsAuthenticated => true;
        public IReadOnlyList<string> Menus => _menus;
        public event EventHandler? SessionChanged;
        public Task InitializeAsync() => Task.CompletedTask;
        public Task<string?> GetTokenAsync() => Task.FromResult<string?>("token");
        public Task<string?> GetRefreshTokenAsync() => Task.FromResult<string?>("refresh");
        public Task LoginAsync(string accessToken, string refreshToken) => Task.CompletedTask;
        public Task SetMenusAsync(IReadOnlyList<string> menus) => Task.CompletedTask;
        public Task LogoutAsync() => Task.CompletedTask;
    }
}
