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

public class AdmitPetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new AdmitPetCommandHandler(hospRepository, unitOfWork);
        var command = new AdmitPetCommand(Guid.NewGuid(), Guid.NewGuid(), "Fever");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await hospRepository.Received(1).AddAsync(Arg.Any<Hospitalization>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
