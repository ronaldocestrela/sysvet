using Core.Domain;
using Core.Infrastructure.Persistence;
using Intelligence.Domain.Entities;
using Intelligence.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intelligence.Infrastructure.Persistence;

/// <summary>EF Core context for Intelligence module (dashboard layouts).</summary>
public class IntelligenceDbContext : DbContext, IIntelligenceUnitOfWork, IDomainEventSource, IUnitOfWork
{
    /// <summary>Profile dashboard layouts.</summary>
    public DbSet<ProfileDashboardLayout> ProfileDashboardLayouts => Set<ProfileDashboardLayout>();

    /// <summary>Current tenant for schema and filters.</summary>
    public ITenantContext TenantContext { get; }

    private readonly ITenantContext _tenantContext;

    /// <summary>Creates the context bound to the current tenant schema.</summary>
    public IntelligenceDbContext(DbContextOptions<IntelligenceDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        TenantContext = _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<AggregateRoot> GetAggregateRootsWithPendingEvents() =>
        ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

    /// <inheritdoc />
    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_tenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntelligenceDbContext).Assembly);
        modelBuilder.ApplyTenantIsolationFilters(this);
        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
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
