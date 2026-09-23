using Core.Application.Messaging;
using Core.Domain;
using Platform.Application.Subscriptions;

namespace Platform.Application.Subscriptions;

/// <summary>Lists catalog plans.</summary>
public sealed record ListPlansQuery : IQuery<IReadOnlyList<PlanSummaryDto>>;

/// <summary>Lists catalog add-ons.</summary>
public sealed record ListAddOnsQuery : IQuery<IReadOnlyList<AddOnSummaryDto>>;

/// <summary>Gets tenant subscription.</summary>
public sealed record GetTenantSubscriptionQuery(Guid TenantId) : IQuery<TenantSubscriptionDto>;

/// <summary>Gets effective entitlements.</summary>
public sealed record GetTenantEntitlementsQuery(Guid TenantId) : IQuery<TenantEntitlementsDto>;
