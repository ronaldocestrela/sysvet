using FluentAssertions;
using NSubstitute;
using Xunit;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using Veterinary.Application.Hospitalizations.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Veterinary.Tests.Application;

public class ExecutePrescriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var prescriptionRepository = Substitute.For<IPrescriptionExecutionRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fever").Value;
        hospRepository.GetByIdAsync(hosp.Id, Arg.Any<CancellationToken>()).Returns(hosp);

        var handler = new ExecutePrescriptionCommandHandler(hospRepository, prescriptionRepository, unitOfWork);
        var command = new ExecutePrescriptionCommand(hosp.Id, "Dipyrone", "500mg", "No side effects", Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await prescriptionRepository.Received(1).AddAsync(Arg.Any<PrescriptionExecution>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        hosp.PrescriptionExecutions.Should().HaveCount(1);
    }
}
