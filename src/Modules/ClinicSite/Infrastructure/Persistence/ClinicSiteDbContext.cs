using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Repositories;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Infrastructure.Persistence;

/// <summary>
/// EF Core context for clinic public site content and global slug index.
/// </summary>
public class ClinicSiteDbContext : DbContext, IClinicSiteUnitOfWork, IDomainEventSource
{
    public DbSet<ClinicSiteProfile> ClinicSiteProfiles => Set<ClinicSiteProfile>();
    public DbSet<ClinicSiteServiceItem> ClinicSiteServiceItems => Set<ClinicSiteServiceItem>();
    public DbSet<ClinicSiteTeamMember> ClinicSiteTeamMembers => Set<ClinicSiteTeamMember>();
    public DbSet<ClinicSiteOpeningHours> ClinicSiteOpeningHours => Set<ClinicSiteOpeningHours>();
    public DbSet<ClinicSiteSlugIndex> ClinicSiteSlugIndexes => Set<ClinicSiteSlugIndex>();

    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Creates the ClinicSite module database context bound to the current tenant schema.
    /// </summary>
    public ClinicSiteDbContext(DbContextOptions<ClinicSiteDbContext> options, ITenantContext tenantContext)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicSiteDbContext).Assembly);
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
