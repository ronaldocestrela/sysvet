using Core.Domain;
using Platform.Domain.Entities;

namespace Platform.Application.Abstractions;

/// <summary>Port for SaaS subscription payment gateway (Asaas live; Fake in CI).</summary>
public interface IBillingGateway
{
    /// <summary>Creates or updates customer at gateway.</summary>
    Task<Result<string>> EnsureCustomerAsync(BillingGatewayCustomerRequest request, CancellationToken cancellationToken);

    /// <summary>Creates payment for invoice amount.</summary>
    Task<Result<BillingGatewayPaymentResult>> CreatePaymentAsync(
        BillingGatewayPaymentRequest request,
        CancellationToken cancellationToken);
}

/// <summary>Customer payload for gateway upsert.</summary>
/// <param name="TenantId">Internal tenant id.</param>
/// <param name="Name">Billing name.</param>
/// <param name="Email">Billing e-mail.</param>
/// <param name="CpfCnpj">Digits only.</param>
/// <param name="ExistingGatewayCustomerId">Optional Asaas customer id.</param>
public sealed record BillingGatewayCustomerRequest(
    Guid TenantId,
    string Name,
    string Email,
    string CpfCnpj,
    string? ExistingGatewayCustomerId);

/// <summary>Payment creation payload.</summary>
/// <param name="InvoiceId">External reference sent to gateway.</param>
/// <param name="GatewayCustomerId">Asaas customer id.</param>
/// <param name="Amount">BRL amount.</param>
/// <param name="DueDate">Due date for Pix/boleto.</param>
/// <param name="PaymentMethod">Credit card, Pix or boleto.</param>
/// <param name="CreditCardToken">Required when payment method is credit card.</param>
public sealed record BillingGatewayPaymentRequest(
    Guid InvoiceId,
    string GatewayCustomerId,
    decimal Amount,
    DateOnly DueDate,
    BillingPaymentMethodKind PaymentMethod,
    string? CreditCardToken);

/// <summary>Gateway payment identifiers and presentation data.</summary>
/// <param name="GatewayPaymentId">Asaas payment id.</param>
/// <param name="PixCopyPaste">Pix payload when applicable.</param>
/// <param name="BoletoIdentificationField">Boleto line when applicable.</param>
public sealed record BillingGatewayPaymentResult(
    string GatewayPaymentId,
    string? PixCopyPaste,
    string? BoletoIdentificationField);
