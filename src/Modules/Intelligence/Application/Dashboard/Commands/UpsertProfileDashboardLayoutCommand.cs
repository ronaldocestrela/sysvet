using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Intelligence.Application.Dashboard.Dtos;

namespace Intelligence.Application.Dashboard.Commands;

/// <summary>Replaces dashboard widget layout for an access profile.</summary>
[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.IntelligenceLayoutWrite)]
public sealed record UpsertProfileDashboardLayoutCommand(
    Guid AccessProfileId,
    IReadOnlyList<DashboardWidgetSlotDto> Slots) : ICommand;
