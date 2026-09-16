using System.Text.Json;
using Bunit;
using Clients.Infrastructure.Http;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using SharedUI.Pages;
using SharedUI.Services;
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
        Services.AddSingleton<IToastService, ToastService>();
    }

    [Fact]
    public void Should_Render_Tutors_Header_And_New_Button()
    {
        var emptyPage = new PagedResultDto<TutorDto> { Items = [], Page = 1, PageSize = 10, TotalCount = 0 };
        _mockHttp.When("/api/v1/tutors*")
            .Respond("application/json", JsonSerializer.Serialize(emptyPage));

        var cut = Render<Tutors>();

        cut.Find("h1").TextContent.MarkupMatches("Tutores");
        cut.Find("button.btn-primary").TextContent.MarkupMatches("Novo Tutor");
    }

    [Fact]
    public void Should_Open_Modal_When_New_Button_Clicked()
    {
        var emptyPage = new PagedResultDto<TutorDto> { Items = [], Page = 1, PageSize = 10, TotalCount = 0 };
        _mockHttp.When("/api/v1/tutors*")
            .Respond("application/json", JsonSerializer.Serialize(emptyPage));
        var cut = Render<Tutors>();

        cut.Find("button.btn-primary").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Find(".modal-header h3").TextContent.MarkupMatches("Novo Tutor");
        });
    }
}
