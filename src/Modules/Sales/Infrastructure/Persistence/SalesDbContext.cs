using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sales.Infrastructure.Persistence;

public class SalesDbContext : DbContext, ISalesUnitOfWork
{
    public ITenantContext TenantContext { get; }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();

    public SalesDbContext(DbContextOptions<SalesDbContext> options, ITenantContext tenantContext) : base(options)
    {
        TenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(TenantContext.SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
        
        modelBuilder.Entity<Order>().HasQueryFilter(o => EF.Property<Guid>(o, "TenantId") == TenantContext.TenantId);
        modelBuilder.Entity<CashRegister>().HasQueryFilter(c => EF.Property<Guid>(c, "TenantId") == TenantContext.TenantId);
        
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
            var tenantIdProp = entry.Metadata.FindProperty("TenantId");
            if (tenantIdProp != null)
            {
                entry.Property("TenantId").CurrentValue = TenantContext.TenantId;
            }
        }
    }
}
