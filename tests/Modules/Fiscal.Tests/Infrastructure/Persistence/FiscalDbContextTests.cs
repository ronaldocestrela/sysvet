using Core.Domain;
using Fiscal.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fiscal.Tests.Infrastructure.Persistence;

public class FiscalDbContextTests
{
    [Fact]
    public async Task MigrateAsync_Should_ApplyInitialFiscalWithNoPendingMigrations()
    {
        var options = new DbContextOptionsBuilder<FiscalDbContext>()
            .UseSqlite($"Data Source=file:fiscal-test-{Guid.NewGuid():N}?mode=memory&cache=shared")
            .Options;

        await using var context = new FiscalDbContext(options, new TestTenantContext());
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
