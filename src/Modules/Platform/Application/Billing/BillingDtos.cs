using Platform.Domain.Entities;

namespace Platform.Application.Billing;

/// <summary>Charge result for tenant billing cycle.</summary>
/// <param name="InvoiceId">Created or existing invoice id.</param>
/// <param name="Amount">Invoice amount.</param>
/// <param name="Status">Invoice status after charge attempt.</param>
/// <param name="GatewayPaymentId">Gateway payment id when created.</param>
public sealed record ChargeTenantBillingResultDto(
    Guid InvoiceId,
    decimal Amount,
    BillingInvoiceStatus Status,
    string? GatewayPaymentId);

/// <summary>Billing customer API view.</summary>
public sealed record BillingCustomerDto(
    Guid TenantId,
    string Name,
    string Email,
    string CpfCnpj,
    string? GatewayCustomerId);

/// <summary>Payment method API view.</summary>
public sealed record BillingPaymentMethodDto(
    Guid TenantId,
    BillingPaymentMethodKind Kind,
    bool HasCreditCardToken);

/// <summary>Invoice list item.</summary>
public sealed record BillingInvoiceDto(
    Guid Id,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal Amount,
    BillingInvoiceStatus Status,
    DateTimeOffset? PaidAt,
    List<BillingChargeDto> Charges);

/// <summary>Charge line on invoice.</summary>
public sealed record BillingChargeDto(
    string GatewayPaymentId,
    string? PixCopyPaste,
    string? BoletoIdentificationField);
