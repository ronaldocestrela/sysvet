using Core.Application.Pets.Queries;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Pets;

public class ListPetsQueryHandlerTests
{
    private readonly IPetRepository _petRepository;
    private readonly ListPetsQueryHandler _handler;

    public ListPetsQueryHandlerTests()
    {
        _petRepository = Substitute.For<IPetRepository>();
        _handler = new ListPetsQueryHandler(_petRepository);
    }

    [Fact]
    public async Task Handle_WithTutorFilter_ShouldReturnFilteredPets()
    {
        var tutorId = Guid.NewGuid();
        var id1 = Guid.NewGuid();
        var pet1 = Pet.Create("Rex", PetSpecies.Dog, "Poodle", PetSex.Male, tutorId, id1).Value;

        _petRepository.SearchAsync(1, 10, tutorId, null, Arg.Any<CancellationToken>())
            .Returns(new PagedList<Pet>(new List<Pet> { pet1 }, 1));

        var query = new ListPetsQuery(TutorId: tutorId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().Be("Rex");
    }
}
