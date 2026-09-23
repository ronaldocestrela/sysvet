using Core.Application.Messaging;
using Platform.Domain.Entities;

namespace Platform.Application.Billing;

/// <summary>Charges one tenant when subscription period is due.</summary>
/// <param name="TenantId">Target tenant.</param>
/// <param name="AsOfUtc">Reference instant for due check.</param>
public sealed record ChargeTenantBillingCommand(Guid TenantId, DateTimeOffset AsOfUtc) : ICommand<ChargeTenantBillingResultDto>;

/// <summary>Charges all due tenant subscriptions.</summary>
/// <param name="AsOfUtc">Reference instant.</param>
public sealed record ChargeDueSubscriptionsCommand(DateTimeOffset AsOfUtc) : ICommand<int>;

/// <summary>Upserts billing customer profile and syncs gateway when configured.</summary>
public sealed record UpsertBillingCustomerCommand(
    Guid TenantId,
    string Name,
    string Email,
    string CpfCnpj) : ICommand<BillingCustomerDto>;

/// <summary>Upserts tenant payment method preference.</summary>
public sealed record UpsertBillingPaymentMethodCommand(
    Guid TenantId,
    BillingPaymentMethodKind Kind,
    string? CreditCardToken) : ICommand<BillingPaymentMethodDto>;

/// <summary>Processes Asaas webhook payload.</summary>
/// <param name="AccessToken">Header asaas-access-token.</param>
/// <param name="Payload">Raw webhook body.</param>
public sealed record ProcessAsaasWebhookCommand(string? AccessToken, AsaasWebhookPayload Payload) : ICommand;
