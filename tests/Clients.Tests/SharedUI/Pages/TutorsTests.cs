using System.Text.Json;
using Bunit;
using Clients.Infrastructure.Http;
using Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using SharedUI.Pages;
using Xunit;

namespace Clients.Tests.SharedUI.Pages;

public class TutorsTests : BunitContext
{
    private readonly MockHttpMessageHandler _mockHttp;

    public TutorsTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");
        
        Services.AddSingleton(new ApiClient(httpClient));
    }

    [Fact]
    public void Should_Render_Tutors_Header_And_New_Button()
    {
        // Arrange
        // Simulate a delay or empty response for the async load
        _mockHttp.When("/api/v1/tutors").Respond("application/json", "[]");
        
        // Act
        var cut = Render<Tutors>();

        // Assert
        cut.Find("h1").TextContent.MarkupMatches("Tutores");
        cut.Find("button.btn-primary").TextContent.MarkupMatches("Novo Tutor");
    }

    [Fact]
    public void Should_Open_Modal_When_New_Button_Clicked()
    {
        // Arrange
        _mockHttp.When("/api/v1/tutors").Respond("application/json", "[]");
        var cut = Render<Tutors>();

        // Act
        cut.Find("button.btn-primary").Click();

        // Assert
        // We look for a modal header or modal content to verify it opened
        cut.Find(".modal-header h3").TextContent.MarkupMatches("Novo Tutor");
    }
}
