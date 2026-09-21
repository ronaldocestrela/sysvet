using Core.Application.IntegrationEvents;
using Core.Domain;
using Inventory.Domain.Repositories;
using InvErrors = Inventory.Domain.ErrorCodes;
using MediatR;

namespace Inventory.Application.PurchaseImports.Integration;

/// <summary>
/// Updates purchase import AP status after Finance materializes payables.
/// </summary>
public sealed class MarkPurchaseInvoiceApLinkedRequestHandler : IRequestHandler<MarkPurchaseInvoiceApLinkedRequest, Result>
{
    private readonly IPurchaseInvoiceImportRepository _importRepository;

    public MarkPurchaseInvoiceApLinkedRequestHandler(IPurchaseInvoiceImportRepository importRepository)
    {
        _importRepository = importRepository;
    }

    public async Task<Result> Handle(MarkPurchaseInvoiceApLinkedRequest request, CancellationToken cancellationToken)
    {
        var import = await _importRepository.GetByIdWithLinesAsync(request.ImportId, cancellationToken);
        if (import is null)
        {
            return Result.Failure(InvErrors.PurchaseImport.NotFound);
        }

        var result = import.MarkApLinked();
        if (result.IsSuccess)
        {
            _importRepository.Update(import);
        }

        return result;
    }
}
