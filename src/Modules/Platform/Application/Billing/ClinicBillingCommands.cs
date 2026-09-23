using Core.Application.Messaging;
using Platform.Domain.Entities;

namespace Platform.Application.Billing;

/// <summary>Clinic admin reads SaaS billing standing (9.5).</summary>
public sealed record GetClinicBillingStandingQuery(Guid TenantId) : IQuery<ClinicBillingStandingDto>;

/// <summary>Clinic admin retries payment for outstanding invoice (9.5).</summary>
public sealed record PayClinicBillingCommand(Guid TenantId, DateTimeOffset AsOfUtc) : ICommand<PayClinicBillingResultDto>;

/// <summary>Standing and open invoice presentation for clinic UI.</summary>
public sealed record ClinicBillingStandingDto(
    BillingStanding Standing,
    bool IsOperationallyLocked,
    Guid? OutstandingInvoiceId,
    decimal? OutstandingAmount,
    BillingInvoiceStatus? OutstandingStatus,
    string? PixCopyPaste,
    string? BoletoLine,
    string? LatestGatewayPaymentId);

/// <summary>Result of manual pay attempt.</summary>
public sealed record PayClinicBillingResultDto(
    Guid InvoiceId,
    decimal Amount,
    string? GatewayPaymentId);
