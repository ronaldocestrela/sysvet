using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Marks a paid order as linked to Finance after receivable title creation (handled by Sales).
/// </summary>
public sealed class MarkOrderFinanceLinkedRequest : IRequest<Result>
{
    public Guid OrderId { get; }

    public MarkOrderFinanceLinkedRequest(Guid orderId)
    {
        OrderId = orderId;
    }
}
