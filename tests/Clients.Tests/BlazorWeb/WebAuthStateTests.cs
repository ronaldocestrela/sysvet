using BlazorWeb.Services;
using Xunit;

namespace Clients.Tests.BlazorWeb;

public class WebAuthStateTests
{
    [Fact]
    public async Task IsAuthenticated_False_Until_Login()
    {
        var sut = new WebAuthState();
        Assert.False(sut.IsAuthenticated);
        Assert.Null(await sut.GetTokenAsync());
    }

    [Fact]
    public async Task Login_Sets_Token_And_Authenticated()
    {
        var sut = new WebAuthState();
        await sut.LoginAsync("jwt-abc");

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("jwt-abc", await sut.GetTokenAsync());
    }

    [Fact]
    public async Task Logout_Clears_Token()
    {
        var sut = new WebAuthState();
        await sut.LoginAsync("jwt-abc");
        await sut.LogoutAsync();

        Assert.False(sut.IsAuthenticated);
        Assert.Null(await sut.GetTokenAsync());
    }
}
