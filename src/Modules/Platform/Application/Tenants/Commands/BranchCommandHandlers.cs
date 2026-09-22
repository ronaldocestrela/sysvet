using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Application.Tenants.Dtos;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using Platform.Domain.ValueObjects;

namespace Platform.Application.Tenants.Commands;

/// <summary>Creates a tenant branch.</summary>
public sealed class AddBranchCommandHandler : IRequestHandler<AddBranchCommand, Result<BranchDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public AddBranchCommandHandler(
        ITenantRepository tenantRepository,
        IBranchRepository branchRepository,
        IPlatformUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<BranchDto>> Handle(AddBranchCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null || tenant.Status == TenantStatus.Deleted)
        {
            return Result.Failure<BranchDto>(Platform.Domain.ErrorCodes.Tenant.NotFound);
        }

        var cnpjResult = BranchCnpj.Create(request.Cnpj);
        if (cnpjResult.IsFailure)
        {
            return Result.Failure<BranchDto>(cnpjResult.Error);
        }

        if (await _branchRepository.CnpjExistsAsync(request.TenantId, cnpjResult.Value.Value, cancellationToken: cancellationToken))
        {
            return Result.Failure<BranchDto>(Platform.Domain.ErrorCodes.Branch.DuplicateCnpj);
        }

        if (request.IsHeadquarters && await _branchRepository.HeadquartersExistsAsync(request.TenantId, cancellationToken))
        {
            return Result.Failure<BranchDto>(Platform.Domain.ErrorCodes.Branch.DuplicateHeadquarters);
        }

        var branchResult = request.IsHeadquarters
            ? Branch.CreateHeadquarters(request.TenantId, request.Cnpj, request.LegalName)
            : Branch.CreateBranch(request.TenantId, request.Cnpj, request.LegalName);

        if (branchResult.IsFailure)
        {
            return Result.Failure<BranchDto>(branchResult.Error);
        }

        await _branchRepository.AddAsync(branchResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(branchResult.Value));
    }

    internal static BranchDto Map(Branch branch) =>
        new(branch.Id, branch.TenantId, branch.Cnpj, branch.LegalName, branch.IsHeadquarters);
}

/// <summary>Updates branch legal name.</summary>
public sealed class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public UpdateBranchCommandHandler(IBranchRepository branchRepository, IPlatformUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.TenantId, request.BranchId, cancellationToken);
        if (branch is null)
        {
            return Result.Failure<BranchDto>(Platform.Domain.ErrorCodes.Branch.NotFound);
        }

        var update = branch.UpdateLegalName(request.LegalName);
        if (update.IsFailure)
        {
            return Result.Failure<BranchDto>(update.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(AddBranchCommandHandler.Map(branch));
    }
}

/// <summary>Soft-deletes branch.</summary>
public sealed class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the handler.</summary>
    public DeleteBranchCommandHandler(IBranchRepository branchRepository, IPlatformUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.TenantId, request.BranchId, cancellationToken);
        if (branch is null)
        {
            return Result.Failure(Platform.Domain.ErrorCodes.Branch.NotFound);
        }

        var deleted = branch.MarkDeleted();
        if (deleted.IsFailure)
        {
            return deleted;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
