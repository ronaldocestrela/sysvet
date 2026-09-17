using System.Text.Json;
using Clients.Infrastructure;
using Clients.Infrastructure.Persistence;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Clients.Tests.Sync;

public class OfflineOutboxTests
{
    private static OfflineDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite($"Data Source=outbox_{Guid.NewGuid()}.db")
            .Options;
        var ctx = new OfflineDbContext(options, new NoOpSqliteFilePersistence());
        ctx.Database.MigrateAsync().GetAwaiter().GetResult();
        return ctx;
    }

    [Fact]
    public async Task SaveChangesAsync_WhenPetAdded_ShouldEnqueueCreatePetCommand()
    {
        await using var ctx = CreateContext();
        var tutor = Tutor.Create("Tutor", Email.Create("t@t.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;
        ctx.Tutors.Add(tutor);
        await ctx.SaveChangesAsync();
        ctx.OutboxMessages.RemoveRange(await ctx.OutboxMessages.ToListAsync());

        var pet = Pet.Create("Rex", PetSpecies.Dog, "Mix", PetSex.Male, tutor.Id).Value;
        ctx.Pets.Add(pet);
        await ctx.SaveChangesAsync();

        var message = await ctx.OutboxMessages.SingleAsync();
        message.Type.Should().Be("CreatePetCommand");
        message.Payload.Should().Contain("IdempotencyKey");
        message.Payload.Should().Contain(message.Id.ToString());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenTutorSoftDeleted_ShouldEnqueueDeleteTutorCommand()
    {
        await using var ctx = CreateContext();
        var tutor = Tutor.Create("Tutor", Email.Create("t@t.com").Value, Cpf.Create("12345678909").Value, Phone.Create("11999999999").Value).Value;
        ctx.Tutors.Add(tutor);
        await ctx.SaveChangesAsync();
        ctx.OutboxMessages.RemoveRange(await ctx.OutboxMessages.ToListAsync());

        tutor.SoftDelete();
        ctx.Update(tutor);
        await ctx.SaveChangesAsync();

        var message = await ctx.OutboxMessages.SingleAsync();
        message.Type.Should().Be("DeleteTutorCommand");
    }

    [Fact]
    public async Task ApplyPullAsync_ShouldNotEnqueueOutbox()
    {
        await using var ctx = CreateContext();
        var applier = new Clients.Infrastructure.Sync.OfflineSyncPullApplier(ctx);
        var page = new Clients.Infrastructure.Sync.ClientPullChangesResult
        {
            Tutors =
            [
                new Clients.Infrastructure.Sync.ClientSyncTutorDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Remote",
                    Email = "remote@test.com",
                    Cpf = "12345678909",
                    Phone = "11888887777",
                    UpdatedAt = DateTimeOffset.UtcNow
                }
            ],
            NextSince = DateTimeOffset.UtcNow
        };

        await applier.ApplyAsync(page);

        (await ctx.OutboxMessages.CountAsync()).Should().Be(0);
        (await ctx.Tutors.CountAsync()).Should().Be(1);
    }
}
