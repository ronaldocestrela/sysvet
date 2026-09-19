using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Events;

namespace Veterinary.Domain.Tests;

public class ClinicalQuoteTests
{
    [Fact]
    public void Send_WithoutItems_ReturnsFailure()
    {
        var quote = ClinicalQuote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = quote.Send();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ClinicalQuote.EmptyItems");
    }

    [Fact]
    public void Send_WithItems_MovesToSent()
    {
        var quote = ClinicalQuote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Consultation", 1m, 150m, ClinicalQuoteItemKind.Service, null, 0)
        ]);

        var result = quote.Send();

        result.IsSuccess.Should().BeTrue();
        quote.Status.Should().Be(ClinicalQuoteStatus.Sent);
        quote.SentAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_FromSent_RaisesDomainEventAndSetsPendingConversion()
    {
        var quoteId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var quote = ClinicalQuote.Create(quoteId, appointmentId, petId, tutorId, Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Exam", 1m, 80m, ClinicalQuoteItemKind.Service, null, 0)
        ]);
        quote.Send();

        var result = quote.Approve();

        result.IsSuccess.Should().BeTrue();
        quote.Status.Should().Be(ClinicalQuoteStatus.Approved);
        quote.ConversionStatus.Should().Be(QuoteConversionStatus.Pending);
        quote.TotalAmount.Should().Be(80m);
        quote.DomainEvents.Should().ContainSingle(e => e is ClinicalQuoteApprovedDomainEvent);
        var domainEvent = (ClinicalQuoteApprovedDomainEvent)quote.DomainEvents.Single();
        domainEvent.QuoteId.Should().Be(quoteId);
        domainEvent.AppointmentId.Should().Be(appointmentId);
        domainEvent.Lines.Should().HaveCount(1);
    }

    [Fact]
    public void Approve_FromDraft_ReturnsFailure()
    {
        var quote = ClinicalQuote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Item", 1m, 10m, ClinicalQuoteItemKind.Product, null, 0)
        ]);

        quote.Approve().IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReplaceDraftItems_AfterSend_ReturnsFailure()
    {
        var quote = ClinicalQuote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Item", 1m, 10m, ClinicalQuoteItemKind.Product, null, 0)
        ]);
        quote.Send();

        quote.ReplaceDraftItems([]).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ApplySyncSnapshot_DoesNotDowngradeApprovedToDraft()
    {
        var id = Guid.NewGuid();
        var quote = ClinicalQuote.Create(id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Item", 1m, 10m, ClinicalQuoteItemKind.Service, null, 0)
        ]);
        quote.Send();
        quote.Approve();

        quote.ApplySyncSnapshot(
            quote.AppointmentId,
            quote.PetId,
            quote.TutorId,
            quote.CreatedByUserId,
            ClinicalQuoteStatus.Draft,
            QuoteConversionStatus.None,
            null,
            "",
            null,
            null,
            DateTimeOffset.UtcNow,
            []);

        quote.Status.Should().Be(ClinicalQuoteStatus.Approved);
        quote.ConversionStatus.Should().Be(QuoteConversionStatus.Pending);
    }

    [Fact]
    public void Reject_FromSent_SetsRejected()
    {
        var quote = ClinicalQuote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Item", 1m, 10m, ClinicalQuoteItemKind.Service, null, 0)
        ]);
        quote.Send();

        quote.Reject().IsSuccess.Should().BeTrue();
        quote.Status.Should().Be(ClinicalQuoteStatus.Rejected);
    }

    [Fact]
    public void RestoreFromSync_RehydratesItems()
    {
        var id = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var restored = ClinicalQuote.RestoreFromSync(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ClinicalQuoteStatus.Sent,
            QuoteConversionStatus.None,
            null,
            "notes",
            now,
            null,
            now,
            [(itemId, "Consulta", 1m, 80m, ClinicalQuoteItemKind.Service, null, 0)]);

        restored.Status.Should().Be(ClinicalQuoteStatus.Sent);
        restored.Items.Should().ContainSingle(i => i.Id == itemId);
    }
}
