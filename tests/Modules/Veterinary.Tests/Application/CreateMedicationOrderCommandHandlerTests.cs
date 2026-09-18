using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Hospitalizations.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class CreateMedicationOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidOrder_ReturnsSuccess()
    {
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fever", DateTimeOffset.UtcNow).Value;
        var repo = Substitute.For<IHospitalizationRepository>();
        repo.GetByIdAsync(hosp.Id, Arg.Any<CancellationToken>()).Returns(hosp);

        var handler = new CreateMedicationOrderCommandHandler(repo);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await handler.Handle(
            new CreateMedicationOrderCommand(hosp.Id, "Dipyrone", "500mg", "IV", [new TimeOnly(8, 0)], today, today),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Received(1).Update(hosp);
    }
}
