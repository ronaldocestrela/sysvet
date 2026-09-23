using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Infrastructure.Billing;
using Platform.Infrastructure.Configuration;

namespace Platform.Tests.Infrastructure;

public class AsaasBillingGatewayTests
{
    [Fact]
    public async Task EnsureCustomerAsync_CreatesCustomer_WhenNew()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":"cus_test123"}""", Encoding.UTF8, "application/json")
        });
        var gateway = CreateGateway(handler);

        var result = await gateway.EnsureCustomerAsync(
            new BillingGatewayCustomerRequest(Guid.NewGuid(), "Clínica", "a@test.com", "39053344705", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("cus_test123");
    }

    [Fact]
    public async Task CreatePaymentAsync_ReturnsPaymentId_WhenSuccess()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":"pay_abc","pixCopyPaste":"pix"}""", Encoding.UTF8, "application/json")
        });
        var gateway = CreateGateway(handler);

        var result = await gateway.CreatePaymentAsync(
            new BillingGatewayPaymentRequest(
                Guid.NewGuid(),
                "cus_test123",
                199m,
                DateOnly.FromDateTime(DateTime.UtcNow),
                BillingPaymentMethodKind.Pix,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.GatewayPaymentId.Should().Be("pay_abc");
    }

    private static AsaasBillingGateway CreateGateway(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        var options = Options.Create(new BillingOptions { ApiKey = "test_key", BaseUrl = "https://api.asaas.com" });
        return new AsaasBillingGateway(client, options, NullLogger<AsaasBillingGateway>.Instance);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> factory) => _factory = factory;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_factory(request));
    }
}
