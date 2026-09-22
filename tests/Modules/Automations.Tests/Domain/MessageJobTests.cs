using Automations.Domain.Entities;
using Automations.Domain.Enums;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class MessageJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Enqueue_StartsPending()
    {
        var result = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "grooming.started",
            """{"TutorName":"Ana"}""",
            "key-1",
            now: Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(MessageJobStatus.Pending);
        result.Value.AttemptCount.Should().Be(0);
        result.Value.NextAttemptAt.Should().Be(Now);
    }

    [Fact]
    public void ScheduleRetry_IncrementsAttemptAndSetsBackoff()
    {
        var job = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "grooming.started",
            "{}",
            "key-2",
            maxAttempts: 5,
            now: Now).Value;

        job.ScheduleRetry("network", Now, Now.AddSeconds(1), Now);

        job.AttemptCount.Should().Be(1);
        job.Status.Should().Be(MessageJobStatus.Failed);
        job.NextAttemptAt.Should().Be(Now.AddSeconds(30));
        job.AttemptLogs.Should().HaveCount(1);
    }

    [Fact]
    public void DeferUntil_DoesNotIncrementAttemptCount()
    {
        var job = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "reminder.vaccine",
            "{}",
            "key-defer",
            now: Now).Value;

        job.DeferUntil(Now.AddHours(2));

        job.AttemptCount.Should().Be(0);
        job.Status.Should().Be(MessageJobStatus.Pending);
        job.NextAttemptAt.Should().Be(Now.AddHours(2));
    }

    [Fact]
    public void MarkDeadLetter_WhenMaxAttemptsReached()
    {
        var job = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "grooming.started",
            "{}",
            "key-3",
            maxAttempts: 1,
            now: Now).Value;

        job.ScheduleRetry("fail", Now, Now.AddSeconds(1), Now);

        job.Status.Should().Be(MessageJobStatus.DeadLetter);
        job.AttemptCount.Should().Be(1);
    }
}
