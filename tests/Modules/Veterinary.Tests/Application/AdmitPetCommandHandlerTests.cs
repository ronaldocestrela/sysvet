using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Hospitalizations.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class AdmitPetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var handler = new AdmitPetCommandHandler(hospRepository);
        var command = new AdmitPetCommand(Guid.NewGuid(), Guid.NewGuid(), "Fever");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await hospRepository.Received(1).AddAsync(Arg.Any<Hospitalization>(), Arg.Any<CancellationToken>());
    }
}
