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
}
