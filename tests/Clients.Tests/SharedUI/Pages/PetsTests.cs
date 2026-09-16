using System.Text.Json;
using Bunit;
using Clients.Infrastructure.Http;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using SharedUI.Pages;
using SharedUI.Services;
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
        Services.AddSingleton<IToastService, ToastService>();
    }

    [Fact]
    public void Should_Render_Pets_Header_And_New_Button()
    {
        var emptyPage = new PagedResultDto<PetDto> { Items = [], Page = 1, PageSize = 10, TotalCount = 0 };
        _mockHttp.When("/api/v1/pets*")
            .Respond("application/json", JsonSerializer.Serialize(emptyPage));

        var cut = Render<Pets>();

        cut.Find("h1").TextContent.MarkupMatches("Pets");
        cut.Find("button.btn-primary").TextContent.MarkupMatches("Novo Pet");
    }

    [Fact]
    public async Task Should_Open_Modal_When_New_Button_Clicked()
    {
        var emptyPage = new PagedResultDto<PetDto> { Items = [], Page = 1, PageSize = 10, TotalCount = 0 };
        var emptyTutors = new PagedResultDto<TutorDto> { Items = [], Page = 1, PageSize = 10, TotalCount = 0 };
        _mockHttp.When("/api/v1/pets*")
            .Respond("application/json", JsonSerializer.Serialize(emptyPage));
        _mockHttp.When("/api/v1/tutors*")
            .Respond("application/json", JsonSerializer.Serialize(emptyTutors));
        var cut = Render<Pets>();

        cut.Find("button.btn-primary").Click();
        cut.WaitForAssertion(() =>
        {
            cut.Find(".modal-header h3").TextContent.MarkupMatches("Novo Pet");
        });
    }
}
