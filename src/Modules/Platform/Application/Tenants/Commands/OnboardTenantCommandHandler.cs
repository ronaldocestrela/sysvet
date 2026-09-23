using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Application.Provisioning;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.ValueObjects;
namespace Platform.Application.Tenants.Commands;

/// <summary>Handles synchronous tenant onboarding (roadmap 9.2).</summary>
public sealed class OnboardTenantCommandHandler : IRequestHandler<OnboardTenantCommand, Result<OnboardTenantResultDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly ITenantProvisioner _provisioner;
    private readonly IIdentityService _identityService;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSubscriptionProvisioner _subscriptionProvisioner;

    /// <summary>Creates the handler.</summary>
    public OnboardTenantCommandHandler(
        ITenantRepository tenantRepository,
        IBranchRepository branchRepository,
        IPlatformUnitOfWork unitOfWork,
        ITenantProvisioner provisioner,
        IIdentityService identityService,
        IAccessProfileRepository accessProfileRepository,
        ITenantContext tenantContext,
        ITenantSubscriptionProvisioner subscriptionProvisioner)
    {
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
        _provisioner = provisioner;
        _identityService = identityService;
        _accessProfileRepository = accessProfileRepository;
        _tenantContext = tenantContext;
        _subscriptionProvisioner = subscriptionProvisioner;
    }

    /// <inheritdoc />
    public async Task<Result<OnboardTenantResultDto>> Handle(OnboardTenantCommand request, CancellationToken cancellationToken)
    {
        var slug = TenantSlug.Normalize(request.Slug);
        if (!TenantSlug.IsValid(slug))
        {
            return Result.Failure<OnboardTenantResultDto>(Platform.Domain.ErrorCodes.Tenant.InvalidSlug);
        }

        if (await _tenantRepository.SlugExistsAsync(slug, cancellationToken))
        {
            return Result.Failure<OnboardTenantResultDto>(Platform.Domain.ErrorCodes.Tenant.DuplicateSlug);
        }

        var tenantId = Guid.NewGuid();
        var tenantResult = Tenant.Create(tenantId, slug, request.DisplayName);
        if (tenantResult.IsFailure)
        {
            return Result.Failure<OnboardTenantResultDto>(tenantResult.Error);
        }

        var branchResult = Branch.CreateHeadquarters(tenantId, request.HeadquartersCnpj, request.HeadquartersLegalName);
        if (branchResult.IsFailure)
        {
            return Result.Failure<OnboardTenantResultDto>(branchResult.Error);
        }

        var tenant = tenantResult.Value;
        var branch = branchResult.Value;

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _branchRepository.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var provision = await _provisioner.ProvisionAsync(tenantId, tenant.SchemaName, cancellationToken);
        if (provision.IsFailure)
        {
            await CompensateCatalogAsync(tenant, branch, cancellationToken);
            return Result.Failure<OnboardTenantResultDto>(provision.Error);
        }

        _tenantContext.TenantId = tenantId;
        _tenantContext.SchemaName = tenant.SchemaName;
        var adminProfile = await _accessProfileRepository.GetSystemProfileByBaseRoleAsync(ApplicationRoles.Admin, cancellationToken);
        if (adminProfile is null)
        {
            await CompensateCatalogAsync(tenant, branch, cancellationToken);
            return Result.Failure<OnboardTenantResultDto>(Platform.Domain.ErrorCodes.Tenant.ProvisioningFailed);
        }

        var userResult = await _identityService.CreateUserAsync(
            request.AdminEmail.Trim(),
            request.AdminPassword,
            ApplicationRoles.Admin,
            tenantId,
            adminProfile.Id,
            displayName: "Admin",
            cancellationToken);

        if (userResult.IsFailure)
        {
            await CompensateCatalogAsync(tenant, branch, cancellationToken);
            return Result.Failure<OnboardTenantResultDto>(userResult.Error);
        }

        var subscription = await _subscriptionProvisioner.ProvisionNewTenantAsync(
            tenantId,
            request.PlanCode,
            request.TrialDays,
            request.TrialEndAction,
            cancellationToken);
        if (subscription.IsFailure)
        {
            await CompensateCatalogAsync(tenant, branch, cancellationToken);
            return Result.Failure<OnboardTenantResultDto>(subscription.Error);
        }

        return Result.Success(new OnboardTenantResultDto(tenantId, userResult.Value, slug));
    }

    private async Task CompensateCatalogAsync(Tenant tenant, Branch branch, CancellationToken cancellationToken)
    {
        await _branchRepository.RemoveAsync(branch, cancellationToken);
        await _tenantRepository.RemoveAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
