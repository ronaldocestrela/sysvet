using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Repositories;

namespace Platform.Application.Tenants.Commands;

/// <summary>Updates tenant status.</summary>
public sealed class ChangeTenantStatusCommandHandler : IRequestHandler<ChangeTenantStatusCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public ChangeTenantStatusCommandHandler(ITenantRepository tenantRepository, IPlatformUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(ChangeTenantStatusCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Tenant.NotFound);
        }

        var change = tenant.ChangeStatus(request.Status);
        if (change.IsFailure)
        {
            return change;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>Soft-deletes tenant.</summary>
public sealed class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public DeleteTenantCommandHandler(ITenantRepository tenantRepository, IPlatformUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Tenant.NotFound);
        }

        var deleted = tenant.MarkDeleted();
        if (deleted.IsFailure)
        {
            return deleted;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
