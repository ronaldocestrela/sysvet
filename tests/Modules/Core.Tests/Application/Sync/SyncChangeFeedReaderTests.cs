using Core.Application.Sync;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Core.Tests.Application.Sync;

public class SyncChangeFeedReaderTests
{
    [Fact]
    public async Task ReadChangesAsync_ShouldIncludeSoftDeletedTombstones()
    {
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlite($"Data Source=sync_feed_{Guid.NewGuid()}.db")
            .Options;

        await using var db = new CoreDbContext(options, new DefaultTenantContext { TenantId = tenantId });
        await db.Database.MigrateAsync();

        var tutor = Tutor.Create("Deleted Tutor",
            Email.Create("del@test.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999999999").Value).Value;
        tutor.SoftDelete();
        db.Tutors.Add(tutor);
        await db.SaveChangesAsync();

        var reader = new SyncChangeFeedReader(db, Array.Empty<ISyncChangeFeedContributor>());
        var page = await reader.ReadChangesAsync(DateTimeOffset.UtcNow.AddHours(-1), 50, CancellationToken.None);

        page.Tutors.Should().ContainSingle(t => t.Id == tutor.Id && t.IsDeleted);
    }
}
