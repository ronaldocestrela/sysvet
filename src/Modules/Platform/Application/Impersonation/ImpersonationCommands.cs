using Core.Application.Messaging;
using Core.Domain;

namespace Platform.Application.Impersonation;

/// <summary>Starts audited impersonation for a tenant (9.6).</summary>
public sealed record StartImpersonationCommand(Guid TargetTenantId, string ClientIp) : ICommand<StartImpersonationResultDto>;

/// <summary>Ends impersonation session.</summary>
public sealed record EndImpersonationCommand(Guid SessionId, string ClientIp) : ICommand;

/// <summary>Lists impersonation audit trail.</summary>
public sealed record ListImpersonationAuditsQuery(int Take = 100) : IQuery<IReadOnlyList<ImpersonationAuditDto>>;

/// <summary>Token returned to Super Admin.</summary>
public sealed record StartImpersonationResultDto(
    string AccessToken,
    int ExpiresInSeconds,
    Guid SessionId,
    Guid TargetTenantId);

/// <summary>Audit list row.</summary>
public sealed record ImpersonationAuditDto(
    Guid Id,
    Guid SessionId,
    string ActorUserId,
    Guid TargetTenantId,
    string Action,
    DateTimeOffset OccurredAt,
    string ClientIp);
