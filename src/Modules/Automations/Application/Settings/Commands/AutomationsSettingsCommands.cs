using Automations.Application.Settings.Dtos;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Automations.Application.Settings.Commands;

/// <summary>
/// Updates tenant business hours for outbound messaging.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record UpdateAutomationsSettingsCommand(
    string TimeZoneId,
    string BusinessStart,
    string BusinessEnd,
    IReadOnlyList<string> BusinessDays,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Updates tutor messaging opt-out flags.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record UpdateTutorMessagingPreferenceCommand(
    Guid TutorId,
    bool WhatsAppEnabled,
    bool EmailEnabled,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Reads tenant Automations settings.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetAutomationsSettingsQuery : IQuery<AutomationsSettingsDto>;

/// <summary>
/// Reads tutor messaging preferences (defaults when absent).
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsRead)]
public sealed record GetTutorMessagingPreferenceQuery(Guid TutorId) : IQuery<TutorMessagingPreferenceDto>;
