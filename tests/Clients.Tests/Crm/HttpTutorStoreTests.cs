using System.Text.Json;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Http;
using FluentAssertions;
using RichardSzalay.MockHttp;

namespace Clients.Tests.Crm;

public class HttpTutorStoreTests
{
    [Fact]
    public async Task ListAsync_ShouldCallTutorsEndpoint()
    {
        var mockHttp = new MockHttpMessageHandler();
        var page = new PagedResultDto<TutorDto>
        {
            Items = [new TutorDto { Id = Guid.NewGuid(), Name = "Remote" }],
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };
        mockHttp.When("/api/v1/tutors?page=1&pageSize=10")
            .Respond("application/json", JsonSerializer.Serialize(page));

        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        var store = new HttpTutorStore(new ApiClient(client));

        var result = await store.ListAsync(1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(t => t.Name == "Remote");
    }
}
