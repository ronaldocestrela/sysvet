using BlazorWeb.Services;
using Xunit;

namespace Clients.Tests.BlazorWeb;

public class WebAuthStateTests
{
    [Fact]
    public async Task IsAuthenticated_False_Until_Login()
    {
        var storage = new InMemoryTokenStorage();
        var sut = new WebAuthState(storage);
        await sut.InitializeAsync();

        Assert.False(sut.IsAuthenticated);
        Assert.Null(await sut.GetTokenAsync());
    }

    [Fact]
    public async Task Login_Sets_Tokens_And_Persists()
    {
        var storage = new InMemoryTokenStorage();
        var sut = new WebAuthState(storage);

        await sut.LoginAsync("jwt-abc", "refresh-xyz");

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("jwt-abc", await sut.GetTokenAsync());
        Assert.Equal("refresh-xyz", await sut.GetRefreshTokenAsync());
        Assert.Equal("jwt-abc", await storage.GetAccessTokenAsync());
    }

    [Fact]
    public async Task Logout_Clears_Tokens()
    {
        var storage = new InMemoryTokenStorage();
        var sut = new WebAuthState(storage);
        await sut.LoginAsync("jwt-abc", "refresh-xyz");
        await sut.LogoutAsync();

        Assert.False(sut.IsAuthenticated);
        Assert.Null(await sut.GetTokenAsync());
        Assert.Null(await storage.GetAccessTokenAsync());
    }

    [Fact]
    public async Task InitializeAsync_Restores_Persisted_Tokens()
    {
        var storage = new InMemoryTokenStorage();
        await storage.SetTokensAsync("stored-access", "stored-refresh");

        var sut = new WebAuthState(storage);
        await sut.InitializeAsync();

        Assert.True(sut.IsAuthenticated);
        Assert.Equal("stored-access", await sut.GetTokenAsync());
    }
}
