using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.Auditing;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the Core module: CRM entities, audit/idempotency stores, and Identity (ADR-001 module boundary).
/// </summary>
public class CoreDbContext : IdentityDbContext<AppUser>, IChangeTrackingUnitOfWork, IDomainEventSource
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
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }

        ConfigureTenantShadowProperty<Tutor>(modelBuilder);
        ConfigureTenantShadowProperty<Pet>(modelBuilder);
        ConfigureTenantShadowProperty<IdempotencyRecord>(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.Property(u => u.TenantId).IsRequired();
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.ToTable("UserRefreshTokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.TokenHash).HasMaxLength(128);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }



    /// <inheritdoc />
    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    /// <inheritdoc />
    public IReadOnlyCollection<AggregateRoot> GetAggregateRootsWithPendingEvents() =>
        ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

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

    private void ConfigureTenantShadowProperty<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        modelBuilder.Entity<TEntity>().Property<Guid>("TenantId");
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == TenantContext.TenantId);
    }

    private void SetTenantIdOnSave()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            var tenantProperty = entry.Metadata.FindProperty("TenantId");
            if (tenantProperty is not null && tenantProperty.IsShadowProperty())
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
