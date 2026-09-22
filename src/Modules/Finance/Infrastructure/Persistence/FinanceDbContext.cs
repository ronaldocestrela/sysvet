using Core.Domain;
using Core.Infrastructure.Persistence;
using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the Finance module (AP/AR titles and cash flow entities).
/// </summary>
public class FinanceDbContext : DbContext, IFinanceUnitOfWork, IDomainEventSource
{
    public DbSet<FinancialTitle> FinancialTitles => Set<FinancialTitle>();
    public DbSet<TitleAllocation> TitleAllocations => Set<TitleAllocation>();
    public DbSet<FinancialCategory> FinancialCategories => Set<FinancialCategory>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<CardReconciliationBatch> CardReconciliationBatches => Set<CardReconciliationBatch>();
    public DbSet<CardReconciliationLine> CardReconciliationLines => Set<CardReconciliationLine>();

    /// <summary>Current tenant for schema and query filters.</summary>
    public ITenantContext TenantContext { get; }

    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Creates the Finance module database context bound to the current tenant schema.
    /// </summary>
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options, ITenantContext tenantContext)
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_tenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
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
