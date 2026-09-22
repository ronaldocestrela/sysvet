using Automations.Domain.Entities;
using Automations.Domain.Enums;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class NpsInviteTests
{
    [Fact]
    public void Respond_ValidScore_Succeeds()
    {
        var now = DateTimeOffset.UtcNow;
        var invite = NpsInvite.Create(
            Guid.NewGuid(),
            "hash",
            now.AddDays(7),
            "Appointment",
            Guid.NewGuid()).Value;

        var result = invite.Respond(9, "Ótimo atendimento", now);
        result.IsSuccess.Should().BeTrue();
        invite.Score.Should().Be(9);
        invite.Status.Should().Be(NpsInviteStatus.Responded);
    }

    [Fact]
    public void Respond_InvalidScore_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        var invite = NpsInvite.Create(
            Guid.NewGuid(),
            "hash",
            now.AddDays(7),
            "Appointment",
            Guid.NewGuid()).Value;

        invite.Respond(11, null, now).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Respond_SecondTime_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        var invite = NpsInvite.Create(
            Guid.NewGuid(),
            "hash",
            now.AddDays(7),
            "Appointment",
            Guid.NewGuid()).Value;

        invite.Respond(8, null, now);
        invite.Respond(8, null, now).IsFailure.Should().BeTrue();
    }
}
