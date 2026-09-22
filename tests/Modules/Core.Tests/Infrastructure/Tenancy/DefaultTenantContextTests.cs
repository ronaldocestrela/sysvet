using Core.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Core.Tests.Infrastructure.Tenancy;

public class DefaultTenantContextTests
{
    [Fact]
    public void SchemaName_Should_UseTenancySettingsDefaultSchema()
    {
        var settings = Microsoft.Extensions.Options.Options.Create(new TenancySettings { DefaultSchema = "tenant_dev" });
        var context = new DefaultTenantContext(settings);

        context.SchemaName.Should().Be("tenant_dev");
    }

    [Fact]
    public void TenantId_Should_UseSingleTenantId_WhenConfigured()
    {
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var settings = Microsoft.Extensions.Options.Options.Create(new TenancySettings { DefaultSchema = "dbo", SingleTenantId = tenantId });
        var context = new DefaultTenantContext(settings);

        context.TenantId.Should().Be(tenantId);
    }
}
