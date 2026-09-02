using Microsoft.EntityFrameworkCore;
using Core.Domain;
using Inventory.Domain.Entities;
using Core.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Persistence;

public interface IInventoryUnitOfWork : IUnitOfWork { }

public class InventoryDbContext : DbContext, IInventoryUnitOfWork
{
    public ITenantContext TenantContext { get; }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductBalance> ProductBalances => Set<ProductBalance>();

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ITenantContext tenantContext) : base(options)
    {
        TenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(TenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        
        modelBuilder.Entity<Product>().HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == TenantContext.TenantId);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(s => EF.Property<Guid>(s, "TenantId") == TenantContext.TenantId);
        modelBuilder.Entity<ProductBalance>().HasQueryFilter(b => EF.Property<Guid>(b, "TenantId") == TenantContext.TenantId);
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdOnSave();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetTenantIdOnSave();
        return base.SaveChanges();
    }

    private void SetTenantIdOnSave()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Metadata.FindProperty("TenantId") != null)
            {
                entry.Property("TenantId").CurrentValue = TenantContext.TenantId;
            }
        }
    }
}
