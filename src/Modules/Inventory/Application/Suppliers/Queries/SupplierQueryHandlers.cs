using Core.Domain;
using Inventory.Application.Suppliers.Dtos;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Suppliers.Queries;

public sealed class ListSuppliersQueryHandler : IRequestHandler<ListSuppliersQuery, Result<IReadOnlyList<SupplierDto>>>
{
    private readonly ISupplierRepository _supplierRepository;

    public ListSuppliersQueryHandler(ISupplierRepository supplierRepository) => _supplierRepository = supplierRepository;

    public async Task<Result<IReadOnlyList<SupplierDto>>> Handle(ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = (await _supplierRepository.GetAllAsync(cancellationToken)).ToList();
        if (request.ActiveOnly)
        {
            suppliers = suppliers.Where(s => s.IsActive).ToList();
        }

        var dtos = suppliers.Select(Map).ToList();
        return Result.Success<IReadOnlyList<SupplierDto>>(dtos);
    }

    private static SupplierDto Map(Domain.Entities.Supplier s) =>
        new(s.Id, s.LegalName, s.TradeName, s.Document, s.ContactEmail, s.ContactPhone, s.IsActive);
}

public sealed class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierByIdQueryHandler(ISupplierRepository supplierRepository) => _supplierRepository = supplierRepository;

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return Result.Failure<SupplierDto>(Inventory.Domain.ErrorCodes.Supplier.NotFound);
        }

        return Result.Success(new SupplierDto(supplier.Id, supplier.LegalName, supplier.TradeName, supplier.Document, supplier.ContactEmail, supplier.ContactPhone, supplier.IsActive));
    }
}
