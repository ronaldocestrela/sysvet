using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Core.Infrastructure.Persistence;

public class TenantAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        if (context is CoreDbContext coreContext)
        {
            return (context.GetType(), coreContext.TenantContext?.SchemaName ?? "dbo", designTime);
        }

        return (context.GetType(), designTime);
    }
}
