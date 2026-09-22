using FluentAssertions;
using TutorPortal.Domain.Entities;

namespace TutorPortal.Tests.Domain;

public class TutorPushSubscriptionTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var result = TutorPushSubscription.Create(
            "user-1",
            "https://push.example/endpoint",
            "p256dh-key",
            "auth-key",
            "Mozilla/5.0");

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be("user-1");
    }

    [Fact]
    public void Create_WithEmptyEndpoint_Fails()
    {
        var result = TutorPushSubscription.Create("user-1", " ", "p256dh", "auth");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TutorPortal.Push.InvalidEndpoint");
    }
}
