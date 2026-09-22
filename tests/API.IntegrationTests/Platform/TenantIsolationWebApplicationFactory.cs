using Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace API.IntegrationTests.Platform;

/// <summary>
/// Shares one SQLite connection so test setup and HTTP requests use the same database.
/// </summary>
public sealed class TenantIsolationWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _disposed;

    public TenantIsolationWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureTestServices(services =>
        {
            ReplaceDbContext<CoreDbContext>(services);
            ReplaceDbContext<global::Platform.Infrastructure.Persistence.PlatformDbContext>(services);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<CoreDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<global::Platform.Infrastructure.Persistence.PlatformDbContext>().Database.EnsureCreated();
        return host;
    }

    private void ReplaceDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        services.RemoveAll<DbContextOptions<TContext>>();
        services.RemoveAll<TContext>();
        services.AddDbContext<TContext>((_, options) =>
        {
            options.UseSqlite(_connection);
            if (typeof(TContext) == typeof(CoreDbContext))
            {
                options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, TenantAwareModelCacheKeyFactory>();
            }
        });
    }

    public new void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connection.Dispose();
        base.Dispose();
    }
}
