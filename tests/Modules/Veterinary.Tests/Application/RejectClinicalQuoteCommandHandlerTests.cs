using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Quotes.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class RejectClinicalQuoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSent_RejectsQuote()
    {
        var quote = ClinicalQuote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        quote.ReplaceDraftItems([
            (Guid.NewGuid(), "Consulta", 1m, 80m, ClinicalQuoteItemKind.Service, null, 0)
        ]);
        quote.Send();

        var repo = Substitute.For<IClinicalQuoteRepository>();
        var audit = Substitute.For<IAuditLogger>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        repo.GetByIdAsync(quote.Id, Arg.Any<CancellationToken>()).Returns(quote);

        var handler = new RejectClinicalQuoteCommandHandler(repo, audit, tenant);
        var result = await handler.Handle(new RejectClinicalQuoteCommand(quote.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        quote.Status.Should().Be(ClinicalQuoteStatus.Rejected);
        repo.Received(1).Update(quote);
    }

    [Fact]
    public async Task Handle_WhenMissing_ReturnsNotFound()
    {
        var repo = Substitute.For<IClinicalQuoteRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ClinicalQuote?)null);

        var handler = new RejectClinicalQuoteCommandHandler(
            repo,
            Substitute.For<IAuditLogger>(),
            Substitute.For<ITenantContext>());

        var result = await handler.Handle(new RejectClinicalQuoteCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ClinicalQuote.NotFound");
    }
}
