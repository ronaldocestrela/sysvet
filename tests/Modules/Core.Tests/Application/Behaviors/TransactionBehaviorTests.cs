using Core.Application.Behaviors;
using Core.Application.Common.Interfaces;
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

    private class TestQuery : IQuery<string>
    {
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenRequestIsNotACommand()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = CreateBehavior<TestQuery>([unitOfWork]);
        var request = new TestQuery();
        var expectedResponse = Result.Success("test");
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(expectedResponse));

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenRequestIsACommandAndResultIsSuccess()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = CreateBehavior<TestCommand>([unitOfWork]);
        var request = new TestCommand();
        var expectedResponse = Result.Success("test");
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(expectedResponse));

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotCallSaveChanges_WhenRequestIsACommandAndResultIsFailure()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = CreateBehavior<TestCommand>([unitOfWork]);
        var request = new TestCommand();
        var expectedResponse = Result.Failure<string>(new Error("Test", "Test Error"));
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(expectedResponse));

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be(expectedResponse);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static TransactionBehavior<TRequest, Result<string>> CreateBehavior<TRequest>(IUnitOfWork[] unitOfWorks)
        where TRequest : IRequest<Result<string>>
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        return new TransactionBehavior<TRequest, Result<string>>(
            unitOfWorks,
            Array.Empty<IDomainEventSource>(),
            dispatcher);
    }
}
