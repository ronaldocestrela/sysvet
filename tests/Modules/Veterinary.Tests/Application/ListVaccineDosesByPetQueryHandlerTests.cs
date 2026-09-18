using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Vaccines.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class ListVaccineDosesByPetQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsDosesForPet()
    {
        var petId = Guid.NewGuid();
        var dose = VaccineDose.Create(Guid.NewGuid(), petId, "Raiva", "B1", DateTimeOffset.UtcNow, null).Value;
        var repository = Substitute.For<IVaccineDoseRepository>();
        repository.GetByPetIdAsync(petId, Arg.Any<CancellationToken>()).Returns([dose]);

        var handler = new ListVaccineDosesByPetQueryHandler(repository);
        var result = await handler.Handle(new ListVaccineDosesByPetQuery(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}
