using System.Threading;
using System.Threading.Tasks;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Behaviors;

public class TransactionBehaviorTests
{
    private class TestCommand : ICommand<string>
    {
    }

    private class TestQuery : IRequest<Result<string>>
    {
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenRequestIsNotACommand()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestQuery, Result<string>>(unitOfWork);
        var request = new TestQuery();
        var expectedResponse = Result.Success("test");
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(expectedResponse));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenRequestIsACommandAndResultIsSuccess()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestCommand, Result<string>>(unitOfWork);
        var request = new TestCommand();
        var expectedResponse = Result.Success("test");
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(expectedResponse));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenRequestIsACommandAndResultIsFailure()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new TransactionBehavior<TestCommand, Result<string>>(unitOfWork);
        var request = new TestCommand();
        var expectedResponse = Result.Failure<string>(new Error("Test", "Test Error"));
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(expectedResponse));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
