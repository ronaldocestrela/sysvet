using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Hospitalizations.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class DischargePetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fever").Value;
        hospRepository.GetByIdAsync(hosp.Id, Arg.Any<CancellationToken>()).Returns(hosp);

        var handler = new DischargePetCommandHandler(hospRepository);
        var command = new DischargePetCommand(hosp.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        hospRepository.Received(1).Update(hosp);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFailure()
    {
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        hospRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Hospitalization?)null);

        var handler = new DischargePetCommandHandler(hospRepository);
        var command = new DischargePetCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.NotFound");
    }
}
