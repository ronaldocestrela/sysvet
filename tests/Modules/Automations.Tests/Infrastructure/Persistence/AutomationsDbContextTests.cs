using Automations.Infrastructure.Persistence;
using Core.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Automations.Tests.Infrastructure.Persistence;

public class AutomationsDbContextTests
{
    [Fact]
    public async Task MigrateAsync_Should_ApplyAutomationsSchemaWithNoPendingMigrations()
    {
        var options = new DbContextOptionsBuilder<AutomationsDbContext>()
            .UseSqlite($"Data Source=file:automations-test-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new AutomationsDbContext(options, new TestTenantContext());
        await context.Database.OpenConnectionAsync();
        await context.Database.MigrateAsync();

        var pending = await context.Database.GetPendingMigrationsAsync();
        pending.Should().BeEmpty();
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
        public string ConnectionString { get; set; } = string.Empty;
    }
}
