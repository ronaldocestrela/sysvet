using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Global platform catalog (schema <c>dbo</c> only — not tenant-scoped).
/// </summary>
public sealed class PlatformDbContext : DbContext, IPlatformUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>Creates the platform catalog context.</summary>
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Unit of work marker for Platform catalog writes.</summary>
public interface IPlatformUnitOfWork : IChangeTrackingUnitOfWork;
