using FluentAssertions;
using NSubstitute;
using Xunit;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using Veterinary.Application.Vaccines.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Veterinary.Tests.Application;

public class RegisterVaccineDoseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var vaccineRepository = Substitute.For<IVaccineDoseRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new RegisterVaccineDoseCommandHandler(vaccineRepository, unitOfWork);
        var command = new RegisterVaccineDoseCommand(Guid.NewGuid(), "Rabies", "B123", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await vaccineRepository.Received(1).AddAsync(Arg.Any<VaccineDose>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
