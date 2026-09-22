using Automations.Domain.Entities;
using Automations.Domain.Repositories;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence;

/// <summary>
/// EF Core context for message templates and outbound job outbox.
/// </summary>
public class AutomationsDbContext : DbContext, IAutomationsUnitOfWork, IDomainEventSource
{
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<MessageJob> MessageJobs => Set<MessageJob>();
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Creates the Automations module database context bound to the current tenant schema.
    /// </summary>
    public AutomationsDbContext(DbContextOptions<AutomationsDbContext> options, ITenantContext tenantContext)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutomationsDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var rowVersion = entityType.FindProperty(nameof(Entity.RowVersion));
            if (rowVersion is not null)
            {
                rowVersion.IsConcurrencyToken = false;
            }
        }

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

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Entity.RowVersion)).IsModified = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
