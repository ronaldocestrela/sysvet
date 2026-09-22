using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the Petshop grooming module.
/// </summary>
public class PetshopDbContext : DbContext, IPetshopUnitOfWork, IDomainEventSource
{
    private readonly ITenantContext _tenantContext;

    /// <summary>Current tenant for schema and query filters.</summary>
    public ITenantContext TenantContext => _tenantContext;

    public PetshopDbContext(DbContextOptions<PetshopDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public DbSet<GroomingAppointment> GroomingAppointments => Set<GroomingAppointment>();
    public DbSet<GroomingSlot> GroomingSlots => Set<GroomingSlot>();
    public DbSet<GroomingRecord> GroomingRecords => Set<GroomingRecord>();
    public DbSet<GroomingRecordSupplyLine> GroomingRecordSupplyLines => Set<GroomingRecordSupplyLine>();
    public DbSet<GroomingService> GroomingServices => Set<GroomingService>();
    public DbSet<GroomingServiceSupplyLine> GroomingServiceSupplyLines => Set<GroomingServiceSupplyLine>();

    /// <inheritdoc />
    public IReadOnlyCollection<AggregateRoot> GetAggregateRootsWithPendingEvents() =>
        ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(_tenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetshopDbContext).Assembly);
        modelBuilder.ApplyTenantIsolationFilters(this);
    }

    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        this.SetTenantIdOnAddedEntities(_tenantContext);
        return await base.SaveChangesAsync(cancellationToken);
    }
}
