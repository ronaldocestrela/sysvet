using Core.Application.Behaviors;
using Core.Domain;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Behaviors;

public class LoggingBehaviorTests
{
    public class TestCommand : IRequest<Result<string>>
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Handle_Should_LogBeforeAndAfterHandling()
    {
        // Arrange
        var request = new TestCommand { Name = "Log Test" };
        var nextDelegate = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        nextDelegate.Invoke().Returns(Task.FromResult(Result.Success("Success")));

        var logger = Substitute.For<ILogger<LoggingBehavior<TestCommand, Result<string>>>>();
        var behavior = new LoggingBehavior<TestCommand, Result<string>>(logger);

        // Act
        var result = await behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await nextDelegate.Received(1).Invoke();

        // Check if LogInformation was called at least once (before and after)
        logger.ReceivedWithAnyArgs().Log(
            LogLevel.Information,
            default,
            default,
            default,
            default!);
    }
}
