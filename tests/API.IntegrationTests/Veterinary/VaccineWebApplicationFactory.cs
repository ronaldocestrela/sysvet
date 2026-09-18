using Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.IntegrationTests.Veterinary;

/// <summary>
/// Uses one open SQLite connection so API requests and test setup share the same in-memory database.
/// </summary>
public sealed class VaccineWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _disposed;

    public VaccineWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<CoreDbContext>>();
            services.RemoveAll<CoreDbContext>();
            services.AddDbContext<CoreDbContext>((_, options) =>
            {
                options.UseSqlite(_connection);
                options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, TenantAwareModelCacheKeyFactory>();
            });

            services.RemoveAll<DbContextOptions<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>>();
            services.RemoveAll<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
            services.AddDbContext<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>((_, options) =>
                options.UseSqlite(_connection));
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
