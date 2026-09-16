using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Infrastructure.Persistence;

public class CoreDbContextMigrationTests
{
    private static CoreDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlite(connection)
            .Options;

        return new CoreDbContext(options, new TestTenantContext());
    }

    [Fact]
    public async Task MigrateAsync_Should_CreateCoreAndIdentityTables()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using (var context = CreateContext(connection))
        {
            await context.Database.MigrateAsync();
        }

        var tableNames = await GetTableNamesAsync(connection);
        tableNames.Should().Contain("Tutors");
        tableNames.Should().Contain("Pets");
        tableNames.Should().Contain("AuditLogs");
        tableNames.Should().Contain("IdempotencyRecords");
        tableNames.Should().Contain("AspNetRoles");
        tableNames.Should().Contain("AspNetUsers");
    }

    [Fact]
    public async Task MigrateAsync_Should_LeaveNoPendingMigrations()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.MigrateAsync();

        var pending = await context.Database.GetPendingMigrationsAsync();
        pending.Should().BeEmpty();
    }

    private static async Task<List<string>> GetTableNamesAsync(SqliteConnection connection)
    {
        var names = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }
}
