using Commerce.Domain.Entities;
using Commerce.Domain.Repositories;
using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Infrastructure.Persistence;

/// <summary>EF Core context for commerce offers, orders and marketplace outbox.</summary>
public class CommerceDbContext : DbContext, ICommerceUnitOfWork, IDomainEventSource
{
    public DbSet<ProductOffer> ProductOffers => Set<ProductOffer>();
    public DbSet<OnlineOrder> OnlineOrders => Set<OnlineOrder>();
    public DbSet<OnlineOrderLine> OnlineOrderLines => Set<OnlineOrderLine>();
    public DbSet<MarketplaceSyncJob> MarketplaceSyncJobs => Set<MarketplaceSyncJob>();
    public DbSet<MercadoLivreSettings> MercadoLivreSettings => Set<MercadoLivreSettings>();
    public DbSet<MarketplaceSellerIndex> MarketplaceSellerIndexes => Set<MarketplaceSellerIndex>();

    private readonly ITenantContext _tenantContext;

    /// <summary>Current tenant schema for EF model cache and queries.</summary>
    public ITenantContext TenantContext => _tenantContext;

    public CommerceDbContext(DbContextOptions<CommerceDbContext> options, ITenantContext tenantContext)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommerceDbContext).Assembly);

        modelBuilder.ApplyTenantIsolationFilters(this, typeof(MarketplaceSellerIndex));

        modelBuilder.Entity<MarketplaceSellerIndex>(entity =>
        {
            entity.ToTable("MarketplaceSellerIndexes", "dbo");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MercadoLivreUserId).IsUnique();
            entity.Property(x => x.MercadoLivreUserId).IsRequired();
            entity.Property(x => x.TenantId).IsRequired();
        });

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
