using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Hospitalizations.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class DischargePetCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAdmitted_ReturnsSuccess()
    {
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fever", DateTimeOffset.UtcNow).Value;
        var repository = Substitute.For<IHospitalizationRepository>();
        var audit = Substitute.For<IAuditLogger>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.UserId.Returns(Guid.NewGuid());
        tenant.TenantId.Returns(Guid.NewGuid());

        repository.GetByIdAsync(hosp.Id, Arg.Any<CancellationToken>()).Returns(hosp);

        var handler = new DischargePetCommandHandler(repository, audit, tenant);
        var result = await handler.Handle(new DischargePetCommand(hosp.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.Received(1).Update(hosp);
    }
}
