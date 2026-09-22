using Automations.Infrastructure.Persistence;
using Automations.Infrastructure.Persistence.Seeding;
using Core.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Automations.Tests.Infrastructure;

public class AutomationsTemplateSeedHostedServiceTests
{
    [Fact]
    public async Task StartAsync_WhenDatabaseUnreachable_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(new TestTenantContext());
        services.AddDbContext<AutomationsDbContext>(options =>
            options.UseSqlServer(
                "Server=127.0.0.1,59999;Database=health_check_test;User Id=sa;Password=Invalid!;TrustServerCertificate=True;Connect Timeout=1"));

        await using var provider = services.BuildServiceProvider();
        var sut = new AutomationsTemplateSeedHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<AutomationsTemplateSeedHostedService>.Instance);

        var act = () => sut.StartAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string SchemaName { get; set; } = "dbo";
        public string ConnectionString { get; set; } = string.Empty;
    }
}
