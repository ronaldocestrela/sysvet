using Core.Application.Messaging;

namespace Platform.Application.Health;

/// <summary>Returns operational health metrics for a tenant (9.7).</summary>
public sealed record GetTenantHealthQuery(Guid TenantId) : IQuery<TenantHealthDto>;
