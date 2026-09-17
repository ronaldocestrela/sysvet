using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using SharedUI.Http;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Http;

public class AuthHandlerTests
{
    [Fact]
    public async Task Protected_Request_Attaches_Bearer()
    {
        var storage = new InMemoryTokenStorage();
        await storage.SetTokensAsync("my-jwt", "refresh");
        var authState = new ClientAuthState(storage);
        await authState.InitializeAsync();

        HttpRequestMessage? captured = null;
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Get, "https://api.test/api/v1/tutors")
            .With(req =>
            {
                captured = req;
                return req.Headers.Authorization?.Parameter == "my-jwt";
            })
            .Respond(HttpStatusCode.OK);

        var services = BuildServices(authState, mockHttp);
        var client = services.GetRequiredService<IHttpClientFactory>().CreateClient("API");

        var response = await client.GetAsync("/api/v1/tutors");

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(captured);
        Assert.Equal("Bearer", captured!.Headers.Authorization?.Scheme);
    }

    [Fact]
    public async Task Login_Path_Does_Not_Send_Bearer_Header()
    {
        var storage = new InMemoryTokenStorage();
        await storage.SetTokensAsync("secret", "refresh");
        var authState = new ClientAuthState(storage);
        await authState.InitializeAsync();

        HttpRequestMessage? captured = null;
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "https://api.test/api/v1/auth/login")
            .With(req =>
            {
                captured = req;
                return req.Headers.Authorization is null;
            })
            .Respond(HttpStatusCode.OK);

        var services = BuildServices(authState, mockHttp);
        var client = services.GetRequiredService<IHttpClientFactory>().CreateClient("API");

        await client.PostAsync("/api/v1/auth/login", null);

        Assert.NotNull(captured);
        Assert.Null(captured!.Headers.Authorization);
    }

    private static ServiceProvider BuildServices(IAuthState authState, MockHttpMessageHandler mockHttp)
    {
        var services = new ServiceCollection();
        services.AddSingleton(authState);
        services.AddSingleton<IAuthTokenRefresher, AuthTokenRefresher>();
        services.AddSingleton<ITokenStorage, InMemoryTokenStorage>();
        services.AddTransient<AuthHandler>();
        services.AddHttpClient("Auth", c => c.BaseAddress = new Uri("https://api.test/"))
            .ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler());
        services.AddHttpClient("API", c => c.BaseAddress = new Uri("https://api.test/"))
            .ConfigurePrimaryHttpMessageHandler(() => mockHttp)
            .AddHttpMessageHandler<AuthHandler>();
        return services.BuildServiceProvider();
    }
}
