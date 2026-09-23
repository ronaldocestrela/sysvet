using Core.Domain;
using Platform.Application.Abstractions;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Infrastructure.Billing;

/// <summary>Deterministic billing gateway for CI and local dev (9.4).</summary>
public sealed class FakeBillingGateway : IBillingGateway
{
    /// <inheritdoc />
    public Task<Result<string>> EnsureCustomerAsync(
        BillingGatewayCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var id = request.ExistingGatewayCustomerId ?? $"fake_cus_{request.TenantId:N}";
        return Task.FromResult(Result.Success(id));
    }

    /// <inheritdoc />
    public Task<Result<BillingGatewayPaymentResult>> CreatePaymentAsync(
        BillingGatewayPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var paymentId = $"fake_pay_{request.InvoiceId:N}";
        return Task.FromResult(Result.Success(new BillingGatewayPaymentResult(
            paymentId,
            request.PaymentMethod == Domain.Entities.BillingPaymentMethodKind.Pix ? "00020126580014br.gov.bcb.pix" : null,
            request.PaymentMethod == Domain.Entities.BillingPaymentMethodKind.Boleto ? "23793.38128 60000.000003 00000.000401 1 84340000010000" : null)));
    }
}
