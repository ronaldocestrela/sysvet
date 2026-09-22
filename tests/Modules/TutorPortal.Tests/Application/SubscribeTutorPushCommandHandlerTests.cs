using Core.Application.Common.Interfaces;
using FluentAssertions;
using NSubstitute;
using TutorPortal.Application.Push.Commands;
using TutorPortal.Domain.Entities;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Tests.Application;

public class SubscribeTutorPushCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsSubscription()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns("user-1");

        var repository = Substitute.For<ITutorPushSubscriptionRepository>();
        repository.GetByUserAndEndpointAsync("user-1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((TutorPushSubscription?)null);

        var unitOfWork = Substitute.For<ITutorPortalUnitOfWork>();

        var handler = new SubscribeTutorPushCommandHandler(currentUser, repository, unitOfWork);
        var result = await handler.Handle(
            new SubscribeTutorPushCommand("https://push.test/end", "key1", "key2", "agent"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<TutorPushSubscription>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
