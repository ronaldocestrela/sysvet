using System.Text.Json;
using Bunit;
using Clients.Infrastructure.Http;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using SharedUI.Pages;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class PetsTests : BunitContext
{
    private readonly MockHttpMessageHandler _mockHttp;

    public PetsTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");
        
        Services.AddSingleton(new ApiClient(httpClient));
    }

    [Fact]
    public void Should_Render_Pets_Header_And_New_Button()
    {
        // Arrange
        _mockHttp.When("/api/v1/pets").Respond("application/json", "[]");
        
        // Act
        var cut = Render<Pets>();

        // Assert
        cut.Find("h1").TextContent.MarkupMatches("Pets");
        cut.Find("button.btn-primary").TextContent.MarkupMatches("Novo Pet");
    }

    [Fact]
    public void Should_Open_Modal_When_New_Button_Clicked()
    {
        // Arrange
        _mockHttp.When("/api/v1/pets").Respond("application/json", "[]");
        var cut = Render<Pets>();

        // Act
        cut.Find("button.btn-primary").Click();

        // Assert
        cut.Find(".modal-header h3").TextContent.MarkupMatches("Novo Pet");
    }
}
