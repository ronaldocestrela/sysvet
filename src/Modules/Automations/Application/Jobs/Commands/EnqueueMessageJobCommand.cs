using Automations.Domain.Enums;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Automations.Application.Jobs.Commands;

/// <summary>
/// Enqueues an outbound message job for asynchronous delivery.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AutomationsWrite)]
public sealed record EnqueueMessageJobCommand(
    MessageChannel Channel,
    string TemplateCode,
    string PayloadJson,
    string? SourceType = null,
    Guid? SourceId = null,
    Guid IdempotencyKey = default) : ICommand<Guid>, IIdempotentCommand<Guid>;
