using Core.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Sales.Infrastructure.Persistence;
using Xunit;

namespace Sales.Tests.Infrastructure;

public sealed class OrderIndexModelTests
{
    [Fact]
    public void Orders_HasTenantIdAndPaidAtIndex()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new SalesDbContext(options, new DefaultTenantContext());
        var entity = context.Model.FindEntityType(typeof(Sales.Domain.Entities.Order));
        Assert.NotNull(entity);

        var hasIndex = entity!.GetIndexes().Any(i =>
            i.Properties.Select(p => p.Name).OrderBy(n => n).SequenceEqual(new[] { "PaidAt", "TenantId" }.OrderBy(n => n)));

        Assert.True(hasIndex);
    }
}
