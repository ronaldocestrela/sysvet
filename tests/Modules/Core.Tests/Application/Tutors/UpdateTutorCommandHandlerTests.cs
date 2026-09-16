using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class UpdateTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly UpdateTutorCommandHandler _handler;

    public UpdateTutorCommandHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _handler = new UpdateTutorCommandHandler(_tutorRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateAndReturnSuccess()
    {
        var id = Guid.NewGuid();
        var tutorResult = Tutor.Create("John Doe", Email.Create("john@example.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, id);
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(tutorResult.Value);

        var command = new UpdateTutorCommand(id, "John Smith", "smith@example.com", "11888888888");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tutorRepository.Received(1).Update(Arg.Any<Tutor>());
    }

    [Fact]
    public async Task Handle_WithNonExistingId_ShouldReturnFailure()
    {
        var id = Guid.NewGuid();
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Tutor?)null);

        var command = new UpdateTutorCommand(id, "John Smith", "smith@example.com", "11888888888");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tutor.NotFound");
        _tutorRepository.DidNotReceive().Update(Arg.Any<Tutor>());
    }
}
