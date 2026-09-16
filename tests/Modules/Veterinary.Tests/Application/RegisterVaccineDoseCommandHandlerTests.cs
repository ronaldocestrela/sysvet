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
        var vaccineRepository = Substitute.For<IVaccineDoseRepository>();
        var handler = new RegisterVaccineDoseCommandHandler(vaccineRepository);
        var command = new RegisterVaccineDoseCommand(Guid.NewGuid(), "Rabies", "B123", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await vaccineRepository.Received(1).AddAsync(Arg.Any<VaccineDose>(), Arg.Any<CancellationToken>());
    }
}
