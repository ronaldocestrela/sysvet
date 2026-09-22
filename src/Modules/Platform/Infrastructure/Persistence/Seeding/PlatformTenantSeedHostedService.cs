using Core.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Seeding;

/// <summary>
/// Ensures the development tenant exists in the global catalog (aligned with <see cref="DevelopmentAdminUserSeeder.DevTenantId"/>).
/// </summary>
public sealed class PlatformTenantSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformTenantSeedHostedService> _logger;

    /// <summary>Creates the hosted seeder.</summary>
    public PlatformTenantSeedHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PlatformTenantSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

            await context.Database.EnsureCreatedAsync(cancellationToken);

            var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
            var existing = await repository.GetByIdAsync(DevelopmentAdminUserSeeder.DevTenantId, cancellationToken);
            if (existing is not null)
            {
                return;
            }

            var tenantResult = Tenant.Create(DevelopmentAdminUserSeeder.DevTenantId, "dev");
            if (tenantResult.IsFailure)
            {
                _logger.LogWarning("Platform tenant seed skipped: {Error}", tenantResult.Error.Message);
                return;
            }

            await repository.AddAsync(tenantResult.Value, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Platform catalog seeded dev tenant {TenantId}", DevelopmentAdminUserSeeder.DevTenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Platform tenant catalog seed skipped.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
