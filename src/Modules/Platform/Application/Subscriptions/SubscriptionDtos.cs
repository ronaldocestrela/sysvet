using Core.Domain.Entitlements;
using Platform.Domain.Entities;

namespace Platform.Application.Subscriptions;

/// <summary>Plan catalog summary.</summary>
public sealed record PlanSummaryDto(Guid Id, string Code, string Name, decimal MonthlyPrice, IReadOnlyList<CommercialModule> Modules);

/// <summary>Add-on catalog summary.</summary>
public sealed record AddOnSummaryDto(Guid Id, string Code, string Name, decimal MonthlyPrice, CommercialModule Module);

/// <summary>Tenant subscription view.</summary>
public sealed record TenantSubscriptionDto(
    Guid TenantId,
    Guid PlanId,
    string PlanCode,
    SubscriptionStatus Status,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    DateTimeOffset? TrialEndsAt,
    TrialEndAction TrialEndAction,
    decimal CreditBalance,
    IReadOnlyList<string> ActiveAddOnCodes);

/// <summary>Effective entitlements for a tenant.</summary>
public sealed record TenantEntitlementsDto(Guid TenantId, IReadOnlyList<CommercialModule> Modules);

/// <summary>Proration result from plan change.</summary>
public sealed record PlanChangeResultDto(decimal ProrationAmount, Guid NewPlanId);

/// <summary>Feature flag update body.</summary>
public sealed record FeatureFlagDto(CommercialModule Module, FeatureFlagState State);
