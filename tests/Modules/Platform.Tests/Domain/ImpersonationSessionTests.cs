using FluentAssertions;
using Platform.Domain.Entities;

namespace Platform.Tests.Domain;

public class ImpersonationSessionTests
{
    [Fact]
    public void Start_AndEnd_SessionLifecycle()
    {
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var start = DateTimeOffset.Parse("2026-09-23T12:00:00Z");
        var expires = start.AddMinutes(15);

        var session = ImpersonationSession.Start(id, "actor-1", "support@vetnexus.app", tenantId, "127.0.0.1", start, expires).Value;
        session.IsActive(start.AddMinutes(5)).Should().BeTrue();
        session.IsActive(expires).Should().BeFalse();

        session.End(start.AddMinutes(10)).IsSuccess.Should().BeTrue();
        session.IsActive(start.AddMinutes(11)).Should().BeFalse();
        session.End(start.AddMinutes(12)).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AuditEntry_CreateStarted_IsAppendOnlyShape()
    {
        var sessionId = Guid.NewGuid();
        var entry = ImpersonationAuditEntry.CreateStarted(sessionId, "actor-1", Guid.NewGuid(), "10.0.0.1", DateTimeOffset.UtcNow).Value;
        entry.Action.Should().Be("Started");
        entry.SessionId.Should().Be(sessionId);
    }
}
