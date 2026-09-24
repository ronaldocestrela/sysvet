using Core.Application.Common;
using Core.Domain;
using MediatR;
using Platform.Application.Tenants.Commands;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Application.Tenants.Queries;

/// <summary>Lists tenants.</summary>
public sealed class ListTenantsQueryHandler : IRequestHandler<ListTenantsQuery, Result<PagedResult<TenantSummaryDto>>>
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>Creates the handler.</summary>
    public ListTenantsQueryHandler(ITenantRepository tenantRepository) => _tenantRepository = tenantRepository;

    /// <inheritdoc />
    public async Task<Result<PagedResult<TenantSummaryDto>>> Handle(ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var pageRequest = PageRequest.TryCreate(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<PagedResult<TenantSummaryDto>>(pageRequest.Error);
        }

        var tenants = request.ActiveOnly
            ? await _tenantRepository.ListActiveAsync(cancellationToken)
            : await _tenantRepository.ListAsync(cancellationToken);

        var filtered = tenants
            .Where(t => t.Status != TenantStatus.Deleted || !request.ActiveOnly)
            .Select(t => new TenantSummaryDto(t.Id, t.Slug, t.DisplayName, t.Status, t.ReleaseRing, t.SchemaName, t.UpdatedAt))
            .OrderBy(t => t.DisplayName)
            .ToList();

        var (page, pageSize) = (pageRequest.Value.Page, pageRequest.Value.PageSize);
        var pageItems = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Result.Success(new PagedResult<TenantSummaryDto>(pageItems, page, pageSize, filtered.Count));
    }
}

/// <summary>Gets tenant by id.</summary>
public sealed class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, Result<TenantDetailDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;

    /// <summary>Creates the handler.</summary>
    public GetTenantQueryHandler(ITenantRepository tenantRepository, IBranchRepository branchRepository)
    {
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
    }

    /// <inheritdoc />
    public async Task<Result<TenantDetailDto>> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<TenantDetailDto>(Platform.Domain.ErrorCodes.Tenant.NotFound);
        }

        var branches = await _branchRepository.ListByTenantAsync(request.TenantId, cancellationToken);
        return Result.Success(new TenantDetailDto(
            tenant.Id,
            tenant.Slug,
            tenant.DisplayName,
            tenant.Status,
            tenant.ReleaseRing,
            tenant.SchemaName,
            tenant.UpdatedAt,
            branches.Count));
    }
}

/// <summary>Lists branches.</summary>
public sealed class ListBranchesQueryHandler : IRequestHandler<ListBranchesQuery, Result<IReadOnlyList<BranchDto>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;

    /// <summary>Creates the handler.</summary>
    public ListBranchesQueryHandler(ITenantRepository tenantRepository, IBranchRepository branchRepository)
    {
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<BranchDto>>> Handle(ListBranchesQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure<IReadOnlyList<BranchDto>>(Platform.Domain.ErrorCodes.Tenant.NotFound);
        }

        var branches = await _branchRepository.ListByTenantAsync(request.TenantId, cancellationToken);
        return Result.Success<IReadOnlyList<BranchDto>>(branches.Select(AddBranchCommandHandler.Map).ToList());
    }
}
