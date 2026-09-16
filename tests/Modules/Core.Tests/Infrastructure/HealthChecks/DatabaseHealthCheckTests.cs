using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Persistence;
using Core.Tests.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Core.Tests.Infrastructure.HealthChecks;

public class DatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithValidSqlite_ReturnsHealthy()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new CoreDbContext(options, new TestTenantContext());
        await context.Database.EnsureCreatedAsync();

        var check = new DatabaseHealthCheck(context);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCannotConnect_ReturnsUnhealthyWithoutThrowing()
    {
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlServer(
                "Server=127.0.0.1,59999;Database=health_check_test;User Id=sa;Password=Invalid!;TrustServerCertificate=True;Connect Timeout=1")
            .Options;

        await using var context = new CoreDbContext(options, new TestTenantContext());
        var check = new DatabaseHealthCheck(context);

        var act = async () => await check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        var result = await act.Should().NotThrowAsync();
        result.Subject.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
