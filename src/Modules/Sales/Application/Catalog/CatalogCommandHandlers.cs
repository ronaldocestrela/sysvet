using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

namespace Sales.Application.Catalog;

public sealed class UpsertProductKitCommandHandler : IRequestHandler<UpsertProductKitCommand, Result<Guid>>
{
    private readonly IProductKitRepository _repository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public UpsertProductKitCommandHandler(IProductKitRepository repository, ISalesUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpsertProductKitCommand request, CancellationToken cancellationToken)
    {
        var components = request.Components.Select(c => (c.ProductId, c.QuantityPerKit)).ToList();

        if (request.Id is Guid id && id != Guid.Empty)
        {
            var existing = await _repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
            {
                var created = ProductKit.Create(id, request.Name, request.IsActive, components);
                if (created.IsFailure)
                {
                    return Result.Failure<Guid>(created.Error);
                }

                _repository.Add(created.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(id);
            }

            var nameUpdate = existing.Update(request.Name, request.IsActive);
            if (nameUpdate.IsFailure)
            {
                return Result.Failure<Guid>(nameUpdate.Error);
            }

            var compUpdate = existing.ReplaceComponents(components);
            if (compUpdate.IsFailure)
            {
                return Result.Failure<Guid>(compUpdate.Error);
            }

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(existing.Id);
        }

        var kit = ProductKit.Create(request.Name, components);
        if (kit.IsFailure)
        {
            return Result.Failure<Guid>(kit.Error);
        }

        if (!request.IsActive)
        {
            kit.Value.Update(request.Name, false);
        }

        _repository.Add(kit.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(kit.Value.Id);
    }
}

public sealed class ListProductKitsQueryHandler : IRequestHandler<ListProductKitsQuery, Result<IReadOnlyList<ProductKitDto>>>
{
    private readonly IProductKitRepository _repository;

    public ListProductKitsQueryHandler(IProductKitRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ProductKitDto>>> Handle(ListProductKitsQuery request, CancellationToken cancellationToken)
    {
        var kits = await _repository.ListAllAsync(cancellationToken);
        var dtos = kits.Select(k => new ProductKitDto(
            k.Id,
            k.Name,
            k.IsActive,
            k.Components.Select(c => new ProductKitComponentDto(c.ProductId, c.QuantityPerKit)).ToList())).ToList();
        return Result.Success<IReadOnlyList<ProductKitDto>>(dtos);
    }
}

public sealed class UpsertServicePackageCommandHandler : IRequestHandler<UpsertServicePackageCommand, Result<Guid>>
{
    private readonly IServicePackageRepository _repository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public UpsertServicePackageCommandHandler(IServicePackageRepository repository, ISalesUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpsertServicePackageCommand request, CancellationToken cancellationToken)
    {
        if (request.Id is Guid id && id != Guid.Empty)
        {
            var existing = await _repository.GetByIdAsync(id, cancellationToken);
            if (existing is null)
            {
                var created = ServicePackage.Create(id, request.Name, request.ServiceCode, request.UsesPerUnit, request.IsActive);
                if (created.IsFailure)
                {
                    return Result.Failure<Guid>(created.Error);
                }

                _repository.Add(created.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(id);
            }

            var update = existing.Update(request.Name, request.ServiceCode, request.UsesPerUnit, request.IsActive);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            _repository.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(existing.Id);
        }

        var package = ServicePackage.Create(request.Name, request.ServiceCode, request.UsesPerUnit);
        if (package.IsFailure)
        {
            return Result.Failure<Guid>(package.Error);
        }

        if (!request.IsActive)
        {
            package.Value.Update(request.Name, request.ServiceCode, request.UsesPerUnit, false);
        }

        _repository.Add(package.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(package.Value.Id);
    }
}

public sealed class ListServicePackagesQueryHandler : IRequestHandler<ListServicePackagesQuery, Result<IReadOnlyList<ServicePackageDto>>>
{
    private readonly IServicePackageRepository _repository;

    public ListServicePackagesQueryHandler(IServicePackageRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ServicePackageDto>>> Handle(ListServicePackagesQuery request, CancellationToken cancellationToken)
    {
        var packages = await _repository.ListAllAsync(cancellationToken);
        var dtos = packages
            .Select(p => new ServicePackageDto(p.Id, p.Name, p.ServiceCode, p.UsesPerUnit, p.IsActive))
            .ToList();
        return Result.Success<IReadOnlyList<ServicePackageDto>>(dtos);
    }
}
