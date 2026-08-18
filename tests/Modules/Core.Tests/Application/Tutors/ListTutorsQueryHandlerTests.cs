using Core.Application.Tutors.Queries;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class ListTutorsQueryHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly ListTutorsQueryHandler _handler;

    public ListTutorsQueryHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _handler = new ListTutorsQueryHandler(_tutorRepository);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnPagedTutors()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var tutor1 = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, id1).Value;
        var tutor2 = Tutor.Create("Jane Smith", Email.Create("jane@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11888888888").Value, id2).Value;

        _tutorRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Tutor> { tutor1, tutor2 });

        var query = new ListTutorsQuery(Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
