using Core.Application.Tutors.Queries;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class GetTutorByIdQueryHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly GetTutorByIdQueryHandler _handler;

    public GetTutorByIdQueryHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _handler = new GetTutorByIdQueryHandler(_tutorRepository);
    }

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnTutorDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tutorResult = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, id);
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(tutorResult.Value);

        var query = new GetTutorByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(id);
        result.Value.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var query = new GetTutorByIdQuery(id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tutor.NotFound");
    }
}
