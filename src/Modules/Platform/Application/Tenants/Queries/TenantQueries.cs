using Core.Application.Messaging;
using Platform.Application.Tenants.Dtos;

namespace Platform.Application.Tenants.Queries;

/// <summary>Lists tenants in the catalog.</summary>
public sealed record ListTenantsQuery(
    bool ActiveOnly = false,
    int Page = 1,
    int PageSize = Core.Application.Common.PageRequest.DefaultPageSize) : IQuery<Core.Application.Common.PagedResult<TenantSummaryDto>>;

/// <summary>Gets tenant detail by id.</summary>
public sealed record GetTenantQuery(Guid TenantId) : IQuery<TenantDetailDto>;

/// <summary>Lists branches for a tenant.</summary>
public sealed record ListBranchesQuery(Guid TenantId) : IQuery<IReadOnlyList<BranchDto>>;
