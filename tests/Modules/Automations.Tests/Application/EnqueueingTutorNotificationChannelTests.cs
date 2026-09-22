using Automations.Domain.Enums;
using Automations.Infrastructure.Notifications;
using Automations.Infrastructure.Persistence;
using Automations.Infrastructure.Persistence.Repositories;
using Core.Application.IntegrationEvents;
using Core.Application.Notifications;
using Core.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Automations.Tests.Application;

public class EnqueueingTutorNotificationChannelTests
{
    [Fact]
    public async Task EnqueuesGroomingJob_WhenReadyForPickup()
    {
        await using var ctx = await CreateContextAsync();
        var channel = new EnqueueingTutorNotificationChannel(
            new MessageJobRepository(ctx),
            ctx);

        await channel.NotifyGroomingStatusAsync(
            new TutorGroomingNotification(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                GroomingNotificationKind.ReadyForPickup,
                "11999999999",
                "Maria",
                "Thor"),
            CancellationToken.None);

        var jobs = await ctx.MessageJobs.ToListAsync();
        jobs.Should().HaveCount(1);
        jobs[0].TemplateCode.Should().Be("grooming.ready");
        jobs[0].Status.Should().Be(MessageJobStatus.Pending);
    }

    private static async Task<AutomationsDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AutomationsDbContext>()
            .UseSqlite($"Data Source=file:automations-channel-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;
        var ctx = new AutomationsDbContext(options, new TestTenantContext());
        await ctx.Database.OpenConnectionAsync();
        await ctx.Database.MigrateAsync();
        return ctx;
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
        public string ConnectionString { get; set; } = string.Empty;
    }
}
