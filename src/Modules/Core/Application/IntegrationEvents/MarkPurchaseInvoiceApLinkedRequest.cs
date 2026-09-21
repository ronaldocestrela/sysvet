using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Marks a confirmed purchase import as linked to Finance after payable titles creation (handled by Inventory).
/// </summary>
public sealed class MarkPurchaseInvoiceApLinkedRequest : IRequest<Result>
{
    public Guid ImportId { get; }

    public MarkPurchaseInvoiceApLinkedRequest(Guid importId)
    {
        ImportId = importId;
    }
}
