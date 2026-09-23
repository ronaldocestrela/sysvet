using Core.Application.Messaging;

namespace Platform.Application.Auditing;

/// <summary>Records a clinic staff login attempt for Super Admin review (9.7).</summary>
public sealed record RecordPlatformLoginCommand(
    string Email,
    bool Succeeded,
    Guid? TenantId,
    string ClientIp,
    string UserAgent) : ICommand;

/// <summary>Lists platform login logs.</summary>
public sealed record ListPlatformLoginLogsQuery(
    Guid? TenantId,
    int Page = 1,
    int PageSize = Core.Application.Common.PageRequest.DefaultPageSize) : IQuery<Core.Application.Common.PagedResult<PlatformLoginLogDto>>;

/// <summary>Lists Super Admin configuration change audits.</summary>
public sealed record ListPlatformChangeAuditsQuery(
    Guid? TenantId,
    int Page = 1,
    int PageSize = Core.Application.Common.PageRequest.DefaultPageSize) : IQuery<Core.Application.Common.PagedResult<PlatformChangeAuditDto>>;
