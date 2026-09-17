using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Core.Application.Sync;

/// <summary>
/// Processes a FIFO batch of client outbox messages via existing CRM commands.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.TutorsWrite)]
public sealed record PushSyncBatchCommand(IReadOnlyList<SyncOutboxMessageDto> Messages) : ICommand<SyncPushResult>;
