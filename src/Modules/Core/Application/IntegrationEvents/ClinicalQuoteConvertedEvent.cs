using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published when a clinical quote is converted to a paid sales order (Fase 6.1).
/// </summary>
public sealed class ClinicalQuoteConvertedEvent : INotification
{
    public Guid QuoteId { get; }
    public Guid OrderId { get; }

    public ClinicalQuoteConvertedEvent(Guid quoteId, Guid orderId)
    {
        QuoteId = quoteId;
        OrderId = orderId;
    }
}
