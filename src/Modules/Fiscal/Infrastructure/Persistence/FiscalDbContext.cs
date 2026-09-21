using Core.Domain;
using Fiscal.Domain.Entities;
using Fiscal.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fiscal.Infrastructure.Persistence;

/// <summary>EF Core context for Fiscal module.</summary>
public class FiscalDbContext : DbContext, IFiscalUnitOfWork, IUnitOfWork, IDomainEventSource
{
    public DbSet<IssuerProfile> IssuerProfiles => Set<IssuerProfile>();
    public DbSet<FiscalDocument> FiscalDocuments => Set<FiscalDocument>();
    public DbSet<FiscalDocumentItem> FiscalDocumentItems => Set<FiscalDocumentItem>();
    public DbSet<FiscalCorrectionLetter> FiscalCorrectionLetters => Set<FiscalCorrectionLetter>();

    private readonly ITenantContext _tenantContext;

    public FiscalDbContext(DbContextOptions<FiscalDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public IReadOnlyCollection<AggregateRoot> GetAggregateRootsWithPendingEvents() =>
        ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_tenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FiscalDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

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
