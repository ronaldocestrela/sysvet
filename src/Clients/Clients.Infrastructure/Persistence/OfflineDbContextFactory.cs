using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clients.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so EF Core CLI can build <see cref="OfflineDbContext"/> without a client host.
/// </summary>
public sealed class OfflineDbContextFactory : IDesignTimeDbContextFactory<OfflineDbContext>
{
    /// <inheritdoc />
    public OfflineDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OfflineDbContext>()
            .UseSqlite("Data Source=offline-design.db")
            .Options;

        return new OfflineDbContext(options, new NoOpSqliteFilePersistence());
    }
}
