using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class RegisterTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly RegisterTutorCommandHandler _handler;

    public RegisterTutorCommandHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _handler = new RegisterTutorCommandHandler(_tutorRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessAndStageTutor()
    {
        var command = new RegisterTutorCommand(Guid.NewGuid(), "John Doe", "john@example.com", "12345678909", "11999999999");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tutorRepository.Received(1).Add(Arg.Any<Tutor>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var command = new RegisterTutorCommand(Guid.NewGuid(), "John Doe", "invalid-email", "12345678909", "11999999999");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _tutorRepository.DidNotReceive().Add(Arg.Any<Tutor>());
    }
}
