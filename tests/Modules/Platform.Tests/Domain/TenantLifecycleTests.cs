using FluentAssertions;
using Platform.Domain;
using Platform.Domain.Entities;

namespace Platform.Tests.Domain;

public class TenantLifecycleTests
{
    private static Tenant ActiveTenant()
    {
        var result = Tenant.Create(Guid.NewGuid(), "clinica-a", "Clínica A");
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public void Suspend_FromActive_Succeeds()
    {
        var tenant = ActiveTenant();
        tenant.Suspend().IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public void Suspend_FromDeleted_Fails()
    {
        var tenant = ActiveTenant();
        tenant.MarkDeleted().IsSuccess.Should().BeTrue();
        tenant.Suspend().IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_FromSuspended_Succeeds()
    {
        var tenant = ActiveTenant();
        tenant.Suspend();
        tenant.Reactivate().IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Cancel_FromActive_Succeeds()
    {
        var tenant = ActiveTenant();
        tenant.Cancel().IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Cancelled);
    }

    [Fact]
    public void MarkDeleted_SetsDeletedAt()
    {
        var tenant = ActiveTenant();
        tenant.MarkDeleted().IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Deleted);
        tenant.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithEmptyDisplayName_Fails()
    {
        var result = Tenant.Create(Guid.NewGuid(), "clinica-b", " ");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Tenant.InvalidDisplayName.Code);
    }
}
