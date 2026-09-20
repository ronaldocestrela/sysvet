using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
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
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentRefund> PaymentRefunds => Set<PaymentRefund>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<CommissionAccrual> CommissionAccruals => Set<CommissionAccrual>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<SaleReturnLine> SaleReturnLines => Set<SaleReturnLine>();
    public DbSet<ProductKit> ProductKits => Set<ProductKit>();
    public DbSet<KitComponent> KitComponents => Set<KitComponent>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<PrepaidBalance> PrepaidBalances => Set<PrepaidBalance>();
    public DbSet<PrepaidCredit> PrepaidCredits => Set<PrepaidCredit>();

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
        modelBuilder.Entity<CommissionRule>().HasQueryFilter(r => EF.Property<Guid>(r, "TenantId") == TenantContext.TenantId);
        modelBuilder.Entity<ProductKit>().HasQueryFilter(k => EF.Property<Guid>(k, "TenantId") == TenantContext.TenantId);
        modelBuilder.Entity<ServicePackage>().HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == TenantContext.TenantId);
        modelBuilder.Entity<PrepaidBalance>().HasQueryFilter(b => EF.Property<Guid>(b, "TenantId") == TenantContext.TenantId);
        
        base.OnModelCreating(modelBuilder);
    }

    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdOnSave();
        PromoteNewEntitiesWithEmptyRowVersion();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetTenantIdOnSave();
        PromoteNewEntitiesWithEmptyRowVersion();
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

    private void PromoteNewEntitiesWithEmptyRowVersion()
    {
        foreach (var entry in ChangeTracker.Entries<Entity>()
                     .Where(e => e.State == EntityState.Modified && e.Entity.RowVersion.Length == 0))
        {
            if (entry.Entity is CommissionAccrual { Status: not CommissionAccrualStatus.Accrued })
            {
                continue;
            }

            if (entry.Entity is Payment or PaymentRefund or SaleReturn or SaleReturnLine or CommissionAccrual
                or KitComponent)
            {
                entry.State = EntityState.Added;
            }
        }
    }
}
