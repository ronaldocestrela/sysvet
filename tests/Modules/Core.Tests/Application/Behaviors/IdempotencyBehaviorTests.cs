using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Behaviors;

public class IdempotencyBehaviorTests
{
    public class TestCommand : IIdempotentCommand<Result<string>>
    {
        public Guid IdempotencyKey { get; }
        public string Data { get; }

        public TestCommand(Guid key, string data)
        {
            IdempotencyKey = key;
            Data = data;
        }
    }

    [Fact]
    public async Task Handle_WhenKeyIsNew_ShouldCallNextAndSaveRecord()
    {
        // Arrange
        var mockService = Substitute.For<IIdempotencyService>();
        mockService.RequestExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        
        var behavior = new IdempotencyBehavior<TestCommand, Result<string>>(mockService);

        var key = Guid.NewGuid();
        var command = new TestCommand(key, "data");

        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Success("Ok")));

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Ok", result.Value);
        
        await mockService.Received(1).CreateRequestAsync(key, "TestCommand", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenKeyExists_ShouldNotCallNextAndReturnSuccess()
    {
        // Arrange
        var mockService = Substitute.For<IIdempotencyService>();
        mockService.RequestExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        var behavior = new IdempotencyBehavior<TestCommand, Result<string>>(mockService);

        var key = Guid.NewGuid();
        var command = new TestCommand(key, "data");

        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Success("Ok")));

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await next.DidNotReceive().Invoke();
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value); // By default it returns default instance of generic type, which is null for string
    }
}
