using FluentAssertions;
using Platform.Domain.Entities;

namespace Platform.Tests.Domain;

public class SaasServiceInvoiceTests
{
    [Fact]
    public void CreatePending_WithValidData_Succeeds()
    {
        var invoiceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var result = SaasServiceInvoice.CreatePending(invoiceId, tenantId, 99.90m, "11222333000181", "Clínica Teste LTDA");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SaasServiceInvoiceStatus.Pending);
    }

    [Fact]
    public void CreatePending_WithZeroAmount_Fails()
    {
        var result = SaasServiceInvoice.CreatePending(Guid.NewGuid(), Guid.NewGuid(), 0m, "11222333000181", "Clínica");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MarkAuthorized_IsIdempotent()
    {
        var row = SaasServiceInvoice.CreatePending(Guid.NewGuid(), Guid.NewGuid(), 10m, "11222333000181", "Clínica").Value;
        row.MarkAuthorized("1", "KEY", "platform/nfse/x.xml").IsSuccess.Should().BeTrue();
        row.MarkAuthorized("1", "KEY", "platform/nfse/x.xml").IsSuccess.Should().BeTrue();
        row.Status.Should().Be(SaasServiceInvoiceStatus.Authorized);
    }

    [Fact]
    public void MarkFailed_AllowsRetryReset()
    {
        var row = SaasServiceInvoice.CreatePending(Guid.NewGuid(), Guid.NewGuid(), 10m, "11222333000181", "Clínica").Value;
        row.MarkFailed("gateway down").IsSuccess.Should().BeTrue();
        row.ResetForRetry().IsSuccess.Should().BeTrue();
        row.Status.Should().Be(SaasServiceInvoiceStatus.Pending);
    }
}
