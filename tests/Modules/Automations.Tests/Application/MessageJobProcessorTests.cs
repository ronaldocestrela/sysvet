using Automations.Application.Abstractions;
using Automations.Application.Jobs;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Domain;
using FluentAssertions;
using NSubstitute;

namespace Automations.Tests.Application;

public class MessageJobProcessorTests
{
    [Fact]
    public async Task ProcessOneAsync_ProcessesPending_MarksSucceeded()
    {
        var job = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "test.hello",
            """{"TutorName":"Ana","PetName":"Thor"}""",
            "proc-success").Value;

        var (processor, jobRepo, uow) = CreateProcessor(new SuccessSender(), job);
        await processor.ProcessOneAsync(job.Id, DateTimeOffset.UtcNow, CancellationToken.None);

        job.Status.Should().Be(MessageJobStatus.Succeeded);
        job.AttemptLogs.Should().HaveCount(1);
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await jobRepo.Received(1).GetByIdAsync(job.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessOneAsync_OnFailure_SchedulesRetry()
    {
        var job = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "test.hello",
            """{"TutorName":"Ana","PetName":"Thor"}""",
            "proc-retry",
            maxAttempts: 5).Value;

        var (processor, _, _) = CreateProcessor(new FailingSender(), job);
        await processor.ProcessOneAsync(job.Id, DateTimeOffset.UtcNow, CancellationToken.None);

        job.Status.Should().Be(MessageJobStatus.Failed);
        job.NextAttemptAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ProcessOneAsync_AfterMaxAttempts_MarksDeadLetter()
    {
        var job = MessageJob.Enqueue(
            Guid.NewGuid(),
            MessageChannel.WhatsApp,
            "test.hello",
            """{"TutorName":"Ana","PetName":"Thor"}""",
            "proc-dead",
            maxAttempts: 1).Value;

        var (processor, _, _) = CreateProcessor(new FailingSender(), job);
        await processor.ProcessOneAsync(job.Id, DateTimeOffset.UtcNow, CancellationToken.None);

        job.Status.Should().Be(MessageJobStatus.DeadLetter);
    }

    private static (MessageJobProcessor Processor, IMessageJobRepository JobRepo, IAutomationsUnitOfWork Uow) CreateProcessor(
        IOutboundMessageSender sender,
        MessageJob job)
    {
        var template = MessageTemplate.Create(
            "test.hello",
            MessageChannel.WhatsApp,
            "Olá {{TutorName}}, pet {{PetName}}.").Value;

        var jobRepo = Substitute.For<IMessageJobRepository>();
        jobRepo.GetByIdAsync(job.Id, Arg.Any<CancellationToken>()).Returns(job);

        var templateRepo = Substitute.For<IMessageTemplateRepository>();
        templateRepo.GetByCodeAndChannelAsync("test.hello", MessageChannel.WhatsApp, Arg.Any<CancellationToken>())
            .Returns(template);

        var uow = Substitute.For<IAutomationsUnitOfWork>();
        var processor = new MessageJobProcessor(jobRepo, templateRepo, sender, uow);
        return (processor, jobRepo, uow);
    }

    private sealed class SuccessSender : IOutboundMessageSender
    {
        public Task<Result> SendAsync(MessageChannel channel, string? subject, string body, string payloadJson, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class FailingSender : IOutboundMessageSender
    {
        public Task<Result> SendAsync(MessageChannel channel, string? subject, string body, string payloadJson, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Failure(new Error("Test.SendFailed", "Simulated failure")));
    }
}
