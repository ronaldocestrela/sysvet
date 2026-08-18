using Core.Application.Tutors.Commands;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Tutors;

public class RegisterTutorCommandHandlerTests
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterTutorCommandHandler _handler;

    public RegisterTutorCommandHandlerTests()
    {
        _tutorRepository = Substitute.For<ITutorRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RegisterTutorCommandHandler(_tutorRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessAndSave()
    {
        // Arrange
        var command = new RegisterTutorCommand("John Doe", "john@example.com", "12345678909", "11999999999");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tutorRepository.Received(1).Add(Arg.Any<Tutor>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterTutorCommand("John Doe", "invalid-email", "12345678909", "11999999999");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _tutorRepository.DidNotReceive().Add(Arg.Any<Tutor>());
    }
}
