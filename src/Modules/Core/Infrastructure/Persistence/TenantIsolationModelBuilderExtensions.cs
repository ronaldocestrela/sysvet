using System.Linq.Expressions;
using System.Reflection;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence;

/// <summary>
/// Shared EF tenant shadow column and query filters (ADR-003 defense in depth on shared SQLite/CI).
/// </summary>
public static class TenantIsolationModelBuilderExtensions
{
    /// <summary>
    /// Applies shadow <c>TenantId</c> and query filters to entity types inheriting <see cref="Entity"/>.
    /// The filter reads <c>TenantContext.TenantId</c> on the <paramref name="dbContext"/> instance so EF
    /// rebinds it per request (a captured <see cref="ITenantContext"/> would freeze the first cached model).
    /// </summary>
    public static void ApplyTenantIsolationFilters(
        this ModelBuilder modelBuilder,
        DbContext dbContext,
        params Type[] excludedClrTypes)
    {
        var excluded = excludedClrTypes.ToHashSet();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null || excluded.Contains(clrType))
            {
                continue;
            }

            if (entityType.IsOwned())
            {
                continue;
            }

            if (!typeof(Entity).IsAssignableFrom(clrType))
            {
                continue;
            }

            var tenantIdProperty = entityType.FindProperty("TenantId");
            if (tenantIdProperty is not null && !tenantIdProperty.IsShadowProperty())
            {
                continue;
            }

            // Required dependents with their own filter make EF hide principals that have no
            // matching children (prescription with zero items, title without allocations, etc.).
            var isRequiredDependent = entityType.GetForeignKeys().Any(fk =>
                fk.IsRequired && !fk.IsOwnership && fk.PrincipalEntityType != entityType);
            if (isRequiredDependent)
            {
                modelBuilder.Entity(clrType).Property<Guid>("TenantId");
                continue;
            }

            ConfigureTenantShadowProperty(modelBuilder, dbContext, clrType);
        }
    }

    /// <summary>
    /// Configures shadow tenant column and filter for a single entity type.
    /// </summary>
    public static void ConfigureTenantShadowProperty(
        ModelBuilder modelBuilder,
        DbContext dbContext,
        Type clrType)
    {
        var method = typeof(TenantIsolationModelBuilderExtensions)
            .GetMethod(nameof(ConfigureTenantShadowPropertyGeneric), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(clrType);
        method.Invoke(null, [modelBuilder, dbContext]);
    }

    private static void ConfigureTenantShadowPropertyGeneric<TEntity>(
        ModelBuilder modelBuilder,
        DbContext dbContext)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>().Property<Guid>("TenantId");

        var entity = Expression.Parameter(typeof(TEntity), "e");
        var tenantId = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(Guid)],
            entity,
            Expression.Constant("TenantId"));

        var currentTenantId = Expression.Property(
            Expression.Property(
                Expression.Constant(dbContext, dbContext.GetType()),
                "TenantContext"),
            nameof(ITenantContext.TenantId));

        Expression body = Expression.Equal(tenantId, currentTenantId);
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            var isDeleted = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                [typeof(bool)],
                entity,
                Expression.Constant(nameof(ISoftDeletable.IsDeleted)));
            body = Expression.AndAlso(body, Expression.Equal(isDeleted, Expression.Constant(false)));
        }

        var filter = Expression.Lambda<Func<TEntity, bool>>(body, entity);
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
    }

    /// <summary>
    /// Sets shadow tenant id on newly inserted entities that expose <c>TenantId</c>.
    /// </summary>
    public static void SetTenantIdOnAddedEntities(this DbContext context, ITenantContext tenantContext)
    {
        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            var tenantProperty = entry.Metadata.FindProperty("TenantId");
            if (tenantProperty is not null && tenantProperty.IsShadowProperty())
            {
                entry.Property("TenantId").CurrentValue = tenantContext.TenantId;
            }
        }
    }
}
