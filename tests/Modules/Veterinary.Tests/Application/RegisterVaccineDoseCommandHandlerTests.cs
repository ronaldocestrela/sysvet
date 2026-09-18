using Core.Domain;
using Core.Domain.Auditing;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Vaccines.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class RegisterVaccineDoseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        var petId = Guid.NewGuid();
        var vaccineRepository = Substitute.For<IVaccineDoseRepository>();
        var protocolRepository = Substitute.For<IVaccineProtocolRepository>();
        var petRepository = Substitute.For<IPetRepository>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var tenantContext = Substitute.For<ITenantContext>();
        petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>())
            .Returns(Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, Guid.NewGuid()).Value);

        var handler = new RegisterVaccineDoseCommandHandler(
            vaccineRepository,
            protocolRepository,
            petRepository,
            auditLogger,
            tenantContext);

        var command = new RegisterVaccineDoseCommand(petId, "Rabies", "B123", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await vaccineRepository.Received(1).AddAsync(Arg.Any<VaccineDose>(), Arg.Any<CancellationToken>());
    }
}
