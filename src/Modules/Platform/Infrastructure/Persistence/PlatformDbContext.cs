using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Global platform catalog (schema <c>dbo</c> only — not tenant-scoped).
/// </summary>
public sealed class PlatformDbContext : DbContext, IPlatformUnitOfWork, IChangeTrackingUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>Tenant legal entities (CNPJ).</summary>
    public DbSet<Branch> Branches => Set<Branch>();

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
