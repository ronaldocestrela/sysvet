using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Entitlements;
using Platform.Application.Subscriptions;
using Platform.Domain.Entities;

namespace Platform.Application.Subscriptions;

/// <summary>Changes tenant plan with proration.</summary>
public sealed record ChangeTenantPlanCommand(Guid TenantId, string PlanCode) : ICommand<PlanChangeResultDto>;

/// <summary>Activates an add-on for a tenant.</summary>
public sealed record ActivateTenantAddOnCommand(Guid TenantId, string AddOnCode) : ICommand<decimal>;

/// <summary>Deactivates an add-on for a tenant.</summary>
public sealed record DeactivateTenantAddOnCommand(Guid TenantId, string AddOnCode) : ICommand<decimal>;

/// <summary>Sets a feature flag override.</summary>
public sealed record SetTenantFeatureFlagCommand(Guid TenantId, CommercialModule Module, FeatureFlagState State) : ICommand;

/// <summary>Expires due trials (background job).</summary>
public sealed record ExpireDueTrialsCommand(DateTimeOffset AsOfUtc) : ICommand<int>;
