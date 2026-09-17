using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class CreateTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly CreateTutorCommandHandler _handler;

    public CreateTutorCommandHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _handler = new CreateTutorCommandHandler(_tutorRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessAndStageTutor()
    {
        var command = new CreateTutorCommand(Guid.NewGuid(), "John Doe", "john@example.com", "12345678909", "11999999999");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tutorRepository.Received(1).Add(Arg.Any<Tutor>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var command = new CreateTutorCommand(Guid.NewGuid(), "John Doe", "invalid-email", "12345678909", "11999999999");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _tutorRepository.DidNotReceive().Add(Arg.Any<Tutor>());
    }

    [Fact]
    public async Task Handle_WithDuplicateCpf_ShouldReturnFailure()
    {
        var existing = Tutor.Create("Existing", Email.Create("a@b.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;
        _tutorRepository.GetByCpfAsync("12345678909", Arg.Any<CancellationToken>()).Returns(existing);

        var command = new CreateTutorCommand(Guid.NewGuid(), "John Doe", "john@example.com", "12345678909", "11999999999");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tutor.DuplicateCpf");
    }

    [Fact]
    public async Task Handle_WhenSameIdAlreadyExists_ShouldReturnSuccessWithoutDuplicateError()
    {
        var id = Guid.NewGuid();
        var existing = Tutor.Create("Existing", Email.Create("a@b.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value, id).Value;
        _tutorRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);

        var command = new CreateTutorCommand(id, "John Doe", "john@example.com", "12345678909", "11999999999");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
        _tutorRepository.DidNotReceive().Add(Arg.Any<Tutor>());
    }
}
