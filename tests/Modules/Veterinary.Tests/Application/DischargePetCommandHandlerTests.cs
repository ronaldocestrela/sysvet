using FluentAssertions;
using NSubstitute;
using Xunit;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using Veterinary.Application.Hospitalizations.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Veterinary.Tests.Application;

public class DischargePetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fever").Value;
        hospRepository.GetByIdAsync(hosp.Id, Arg.Any<CancellationToken>()).Returns(hosp);

        var handler = new DischargePetCommandHandler(hospRepository, unitOfWork);
        var command = new DischargePetCommand(hosp.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        hospRepository.Received(1).Update(hosp);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        hospRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Hospitalization?)null);

        var handler = new DischargePetCommandHandler(hospRepository, unitOfWork);
        var command = new DischargePetCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.NotFound");
    }
}
