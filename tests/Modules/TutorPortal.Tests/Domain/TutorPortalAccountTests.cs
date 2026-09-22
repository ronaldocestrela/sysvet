using FluentAssertions;
using TutorPortal.Domain.Entities;

namespace TutorPortal.Tests.Domain;

public class TutorPortalAccountTests
{
    [Fact]
    public void Create_WithValidIds_ShouldSucceed()
    {
        var tutorId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var result = TutorPortalAccount.Create(userId, tutorId);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.TutorId.Should().Be(tutorId);
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        var result = TutorPortalAccount.Create(string.Empty, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TutorPortal.Account.NotFound");
    }

    [Fact]
    public void Create_WithEmptyTutorId_ShouldFail()
    {
        var result = TutorPortalAccount.Create(Guid.NewGuid().ToString(), Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TutorPortal.Account.NotFound");
    }
}
