using Core.Application.Pets.Queries;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Pets;

public class GetPetByIdQueryHandlerTests
{
    private readonly IPetRepository _petRepository;
    private readonly GetPetByIdQueryHandler _handler;

    public GetPetByIdQueryHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _handler = new GetPetByIdQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnPetDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var petResult = Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, Guid.NewGuid(), id);
        _petRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(petResult.Value);

        var query = new GetPetByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be("Rex");
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _petRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Pet?)null);

        var query = new GetPetByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Pet.NotFound");
    }
}
