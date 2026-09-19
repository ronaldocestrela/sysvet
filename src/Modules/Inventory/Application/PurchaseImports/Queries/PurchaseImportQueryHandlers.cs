using Core.Domain;
using Inventory.Application.PurchaseImports.Dtos;
using Inventory.Domain.Repositories;
using PurchaseImportErrors = Inventory.Domain.ErrorCodes.PurchaseImport;
using MediatR;

namespace Inventory.Application.PurchaseImports.Queries;

/// <summary>Loads import detail for conference.</summary>
public sealed class GetPurchaseImportByIdQueryHandler : IRequestHandler<GetPurchaseImportByIdQuery, Result<PurchaseImportDetailDto>>
{
    private readonly IPurchaseInvoiceImportRepository _importRepository;

    public GetPurchaseImportByIdQueryHandler(IPurchaseInvoiceImportRepository importRepository)
        => _importRepository = importRepository;

    public async Task<Result<PurchaseImportDetailDto>> Handle(GetPurchaseImportByIdQuery request, CancellationToken cancellationToken)
    {
        var import = await _importRepository.GetByIdWithLinesAsync(request.ImportId, cancellationToken);
        if (import is null)
        {
            return Result.Failure<PurchaseImportDetailDto>(PurchaseImportErrors.NotFound);
        }

        return Result.Success(MapDetail(import));
    }

    internal static PurchaseImportDetailDto MapDetail(Domain.Entities.PurchaseInvoiceImport import) =>
        new()
        {
            Id = import.Id,
            AccessKey = import.AccessKey,
            Status = import.Status,
            ApIntegrationStatus = import.ApIntegrationStatus,
            SupplierId = import.SupplierId,
            EmitterLegalName = import.EmitterLegalName,
            InvoiceNumber = import.InvoiceNumber,
            InvoiceSeries = import.InvoiceSeries,
            IssuedAt = import.IssuedAt,
            TotalAmount = import.TotalAmount,
            Lines = import.Lines.Select(l => new PurchaseImportLineDetailDto
            {
                LineId = l.Id,
                ItemNumber = l.ItemNumber,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                ProductId = l.ProductId,
                ProductLotId = l.ProductLotId,
                StockMovementId = l.StockMovementId,
                LotNumber = l.LotNumber
            }).ToList()
        };
}

/// <summary>Lists recent imports.</summary>
public sealed class ListPurchaseImportsQueryHandler : IRequestHandler<ListPurchaseImportsQuery, Result<IReadOnlyList<PurchaseImportListItemDto>>>
{
    private readonly IPurchaseInvoiceImportRepository _importRepository;

    public ListPurchaseImportsQueryHandler(IPurchaseInvoiceImportRepository importRepository)
        => _importRepository = importRepository;

    public async Task<Result<IReadOnlyList<PurchaseImportListItemDto>>> Handle(ListPurchaseImportsQuery request, CancellationToken cancellationToken)
    {
        var imports = await _importRepository.ListRecentAsync(request.Take, cancellationToken);
        var items = imports.Select(i => new PurchaseImportListItemDto
        {
            Id = i.Id,
            AccessKey = i.AccessKey,
            Status = i.Status,
            EmitterLegalName = i.EmitterLegalName,
            IssuedAt = i.IssuedAt,
            TotalAmount = i.TotalAmount
        }).ToList();

        return Result.Success<IReadOnlyList<PurchaseImportListItemDto>>(items);
    }
}
