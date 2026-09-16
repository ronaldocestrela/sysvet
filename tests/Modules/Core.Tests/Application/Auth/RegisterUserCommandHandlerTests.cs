using Core.Application.Auth.Commands;
using Core.Application.Common.Interfaces;
using Core.Domain;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _hostEnvironment = Substitute.For<IHostEnvironment>();
        _handler = new RegisterUserCommandHandler(_identityService, _hostEnvironment);
    }

    [Fact]
    public async Task Handle_WhenNotDevelopment_ShouldReturnRegistrationNotAllowed()
    {
        _hostEnvironment.EnvironmentName.Returns(Environments.Production);

        var result = await _handler.Handle(
            new RegisterUserCommand("new@sysvet.com", "Password123!", "Admin"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.RegistrationNotAllowed);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        _hostEnvironment.EnvironmentName.Returns(Environments.Development);
        _identityService
            .CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<string>(ErrorCodes.Auth.DuplicateEmail)));

        var result = await _handler.Handle(
            new RegisterUserCommand("dup@sysvet.com", "Password123!", "Receptionist"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.DuplicateEmail);
    }

    [Fact]
    public async Task Handle_InDevelopment_ShouldReturnUserId()
    {
        _hostEnvironment.EnvironmentName.Returns(Environments.Development);
        var userId = Guid.NewGuid().ToString();
        _identityService
            .CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), "Cashier", Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(userId)));

        var result = await _handler.Handle(
            new RegisterUserCommand("cashier@sysvet.com", "Password123!", "Cashier"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Guid.Parse(userId));
    }
}
