using Core.Domain;
using Core.Domain.Auditing;
using Core.Domain.Entities;
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
        var petId = Guid.NewGuid();
        var bedId = Guid.NewGuid();
        var hospRepository = Substitute.For<IHospitalizationRepository>();
        var wardRepository = Substitute.For<IWardUnitRepository>();
        var petRepository = Substitute.For<IPetRepository>();
        var audit = Substitute.For<IAuditLogger>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.UserId.Returns(Guid.NewGuid());
        tenant.TenantId.Returns(Guid.NewGuid());

        petRepository.GetByIdAsync(petId, Arg.Any<CancellationToken>()).Returns(Pet.Create("Rex", PetSpecies.Dog, "Mix", PetSex.Male, Guid.NewGuid()).Value);
        wardRepository.FindBedAsync(bedId, Arg.Any<CancellationToken>())
            .Returns((WardUnit.Create(Guid.NewGuid(), "ICU").Value, Bed.Create(bedId, Guid.NewGuid(), "A1", 0).Value));
        hospRepository.GetActiveByPetAsync(petId, Arg.Any<CancellationToken>()).Returns((Hospitalization?)null);
        hospRepository.GetActiveByBedAsync(bedId, Arg.Any<CancellationToken>()).Returns((Hospitalization?)null);

        var handler = new AdmitPetCommandHandler(hospRepository, wardRepository, petRepository, audit, tenant);
        var command = new AdmitPetCommand(petId, Guid.NewGuid(), bedId, "Fever");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await hospRepository.Received(1).AddAsync(Arg.Any<Hospitalization>(), Arg.Any<CancellationToken>());
    }
}
