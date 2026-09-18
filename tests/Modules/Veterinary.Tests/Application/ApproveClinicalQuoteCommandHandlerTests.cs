using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Quotes.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Tests;

public class ApproveClinicalQuoteCommandHandlerTests
{
    [Fact]
    public async Task Approve_SentQuote_SetsPendingConversion()
    {
        var quoteId = Guid.NewGuid();
        var quote = ClinicalQuote.Create(quoteId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([(Guid.NewGuid(), "Item", 1m, 50m, ClinicalQuoteItemKind.Service, null, 0)]);
        quote.Send();

        var repository = Substitute.For<IClinicalQuoteRepository>();
        repository.GetByIdAsync(quoteId, Arg.Any<CancellationToken>()).Returns(quote);

        var handler = new ApproveClinicalQuoteCommandHandler(
            repository,
            Substitute.For<IAuditLogger>(),
            Substitute.For<ITenantContext>());

        var result = await handler.Handle(new ApproveClinicalQuoteCommand(quoteId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        quote.ConversionStatus.Should().Be(QuoteConversionStatus.Pending);
        repository.Received(1).Update(quote);
    }
}
