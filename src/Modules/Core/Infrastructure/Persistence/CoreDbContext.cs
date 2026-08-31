using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.Auditing;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Infrastructure.Persistence;

public class CoreDbContext : IdentityDbContext<AppUser>, IUnitOfWork
{
    public ITenantContext TenantContext { get; set; } = null!;

    public CoreDbContext(DbContextOptions<CoreDbContext> options, ITenantContext tenantContext) : base(options)
    {
        TenantContext = tenantContext;
    }

    public string SchemaName => TenantContext?.SchemaName ?? "dbo";

    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);

        modelBuilder.Entity<Tutor>().Property<Guid>("TenantId");
        modelBuilder.Entity<Tutor>().HasQueryFilter(t => EF.Property<Guid>(t, "TenantId") == TenantContext.TenantId);

        modelBuilder.Entity<Pet>().Property<Guid>("TenantId");
        modelBuilder.Entity<Pet>().HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == TenantContext.TenantId);

        modelBuilder.Entity<IdempotencyRecord>().Property<Guid>("TenantId");
        modelBuilder.Entity<IdempotencyRecord>().HasQueryFilter(i => EF.Property<Guid>(i, "TenantId") == TenantContext.TenantId);
    }



    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdOnSave();
        CaptureAuditLogs();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetTenantIdOnSave();
        CaptureAuditLogs();
        return base.SaveChanges();
    }

    private void SetTenantIdOnSave()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            if (entry.Metadata.FindProperty("TenantId") != null)
            {
                entry.Property("TenantId").CurrentValue = TenantContext.TenantId;
            }
        }
    }

    private void CaptureAuditLogs()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            
            var payload = "";
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                payload = System.Text.Json.JsonSerializer.Serialize(values);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                payload = System.Text.Json.JsonSerializer.Serialize(values);
            }

            var auditLogResult = AuditLog.Create(
                TenantContext.TenantId, 
                TenantContext.UserId, 
                entityName, 
                action, 
                payload);

            if (auditLogResult.IsSuccess)
            {
                AuditLogs.Add(auditLogResult.Value);
            }
        }
    }
}
