using Automations.Application.Campaigns.Dtos;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Automations.Application.Campaigns.Commands;

/// <summary>
/// Creates a draft campaign for a built-in segment.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record CreateCampaignCommand(
    string Name,
    CampaignSegmentKind SegmentKind,
    string? TemplateCode = null,
    int InactiveDays = Campaign.DefaultInactiveDays,
    int CooldownDays = Campaign.DefaultCooldownDays,
    Guid IdempotencyKey = default) : ICommand<Guid>, IIdempotentCommand<Guid>;

/// <summary>
/// Updates campaign metadata and status.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record UpdateCampaignCommand(
    Guid CampaignId,
    string Name,
    string TemplateCode,
    CampaignStatus Status,
    int InactiveDays,
    int CooldownDays,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Manually launches an inactive-segment campaign.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record LaunchCampaignCommand(
    Guid CampaignId,
    Guid IdempotencyKey = default) : ICommand<CampaignRunDto>, IIdempotentCommand<CampaignRunDto>;
