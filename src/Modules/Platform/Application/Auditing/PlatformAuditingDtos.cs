namespace Platform.Application.Auditing;

/// <summary>Login log row for Super Admin API.</summary>
public sealed record PlatformLoginLogDto(
    Guid Id,
    Guid? TenantId,
    string Email,
    bool Succeeded,
    string ClientIp,
    string UserAgent,
    string Country,
    string Region,
    DateTimeOffset OccurredAt);

/// <summary>Configuration change audit row for Super Admin API.</summary>
public sealed record PlatformChangeAuditDto(
    Guid Id,
    string ActorUserId,
    Guid? TenantId,
    string Action,
    string PayloadSummary,
    string ClientIp,
    DateTimeOffset OccurredAt);
