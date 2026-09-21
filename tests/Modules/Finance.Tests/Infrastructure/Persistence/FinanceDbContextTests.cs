using Core.Domain;
using Finance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Finance.Tests.Infrastructure.Persistence;

public class FinanceDbContextTests
{
    [Fact]
    public async Task MigrateAsync_Should_ApplyInitialFinanceWithNoPendingMigrations()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseSqlite($"Data Source=file:finance-test-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new FinanceDbContext(options, new TestTenantContext());
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
