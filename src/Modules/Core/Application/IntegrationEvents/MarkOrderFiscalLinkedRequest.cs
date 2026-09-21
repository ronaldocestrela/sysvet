using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Marks sales order fiscal integration after document emission.</summary>
public sealed class MarkOrderFiscalLinkedRequest : IRequest<Result>
{
    public Guid OrderId { get; }
    public bool Partial { get; }

    public MarkOrderFiscalLinkedRequest(Guid orderId, bool partial)
    {
        OrderId = orderId;
        Partial = partial;
    }
}
