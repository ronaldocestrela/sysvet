using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Hospitalizations.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class ExecutePrescriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var prescriptionRepository = Substitute.For<IPrescriptionExecutionRepository>();
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fever").Value;
        hospRepository.GetByIdAsync(hosp.Id, Arg.Any<CancellationToken>()).Returns(hosp);

        var handler = new ExecutePrescriptionCommandHandler(hospRepository, prescriptionRepository);
        var command = new ExecutePrescriptionCommand(hosp.Id, "Dipyrone", "500mg", "No side effects", Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await prescriptionRepository.Received(1).AddAsync(Arg.Any<PrescriptionExecution>(), Arg.Any<CancellationToken>());
        hosp.PrescriptionExecutions.Should().HaveCount(1);
    }
}
