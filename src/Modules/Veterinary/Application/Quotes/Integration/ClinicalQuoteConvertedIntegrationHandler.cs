using Core.Application.IntegrationEvents;
using MediatR;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Quotes.Integration;

/// <summary>
/// Marks clinical quote as converted when PDV completes payment.
/// </summary>
public sealed class ClinicalQuoteConvertedIntegrationHandler : INotificationHandler<ClinicalQuoteConvertedEvent>
{
    private readonly IClinicalQuoteRepository _quoteRepository;

    public ClinicalQuoteConvertedIntegrationHandler(IClinicalQuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    public async Task Handle(ClinicalQuoteConvertedEvent notification, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(notification.QuoteId, cancellationToken);
        if (quote is null)
        {
            return;
        }

        var result = quote.MarkConverted(notification.OrderId);
        if (result.IsSuccess)
        {
            _quoteRepository.Update(quote);
        }
    }
}
