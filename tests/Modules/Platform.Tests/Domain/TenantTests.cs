using Core.Domain;
using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.ValueObjects;

namespace Platform.Tests.Domain;

public class TenantTests
{
    [Fact]
    public void TenantSlug_Create_WithInvalidSlug_Fails()
    {
        var result = TenantSlug.Create("x");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Platform.Tenant.InvalidSlug");
    }

    [Fact]
    public void Tenant_Create_SetsSchemaNameFromId()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var result = Tenant.Create(id, "clinica-teste");

        result.IsSuccess.Should().BeTrue();
        result.Value.SchemaName.Should().Be(TenantSchema.FromId(id));
        result.Value.Slug.Should().Be("clinica-teste");
    }
}
