using Core.Domain;
using Microsoft.EntityFrameworkCore;
using TutorPortal.Domain.Entities;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Infrastructure.Persistence;

/// <summary>
/// EF Core context for tutor portal account linkages.
/// </summary>
public class TutorPortalDbContext : DbContext, ITutorPortalUnitOfWork, IDomainEventSource
{
    public DbSet<TutorPortalAccount> TutorPortalAccounts => Set<TutorPortalAccount>();

    /// <summary>Web Push subscriptions for tutor portal users.</summary>
    public DbSet<TutorPushSubscription> TutorPushSubscriptions => Set<TutorPushSubscription>();

    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Creates the TutorPortal module database context bound to the current tenant schema.
    /// </summary>
    public TutorPortalDbContext(DbContextOptions<TutorPortalDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<AggregateRoot> GetAggregateRootsWithPendingEvents() =>
        ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

    /// <inheritdoc />
    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_tenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TutorPortalDbContext).Assembly);
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

        return await base.SaveChangesAsync(cancellationToken);
    }
}
