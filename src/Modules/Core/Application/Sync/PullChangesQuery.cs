using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Core.Application.Sync;

/// <summary>
/// Pulls CRM changes since a cursor for offline hydration (ADR-002).
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.TutorsRead)]
public sealed record PullChangesQuery(DateTimeOffset Since, int Take = 100) : IQuery<PullChangesResult>;
