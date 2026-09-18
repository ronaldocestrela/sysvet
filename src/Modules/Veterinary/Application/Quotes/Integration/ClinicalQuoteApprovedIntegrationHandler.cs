using Core.Application.IntegrationEvents;
using MediatR;
using Veterinary.Domain.Events;

namespace Veterinary.Application.Quotes.Integration;

/// <summary>Maps approved quote domain events to cross-module integration notifications.</summary>
public sealed class ClinicalQuoteApprovedIntegrationHandler : INotificationHandler<DomainEventEnvelope>
{
    private readonly IPublisher _publisher;

    public ClinicalQuoteApprovedIntegrationHandler(IPublisher publisher) => _publisher = publisher;

    public async Task Handle(DomainEventEnvelope notification, CancellationToken cancellationToken)
    {
        if (notification.DomainEvent is not ClinicalQuoteApprovedDomainEvent approved)
        {
            return;
        }

        var items = approved.Lines
            .Select(l => new ClinicalQuoteApprovedItem(
                l.ItemId,
                l.Description,
                l.Quantity,
                l.UnitPrice,
                l.Kind.ToString(),
                l.ProductId))
            .ToList();

        await _publisher.Publish(
            new ClinicalQuoteApprovedEvent(
                approved.QuoteId,
                approved.AppointmentId,
                approved.PetId,
                approved.TutorId,
                items),
            cancellationToken);
    }
}
