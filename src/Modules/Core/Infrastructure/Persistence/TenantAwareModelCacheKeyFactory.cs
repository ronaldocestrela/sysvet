using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Infrastructure.Persistence;

public class TenantAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        if (context is CoreDbContext coreContext)
        {
            return (context.GetType(), coreContext.TenantContext?.SchemaName ?? "dbo", designTime);
        }

        if (context.GetType().GetProperty("TenantContext")?.GetValue(context) is ITenantContext tenantContext
            && !string.IsNullOrWhiteSpace(tenantContext.SchemaName))
        {
            return (context.GetType(), tenantContext.SchemaName, designTime);
        }

        return (context.GetType(), designTime);
    }
}
