using Core.Application.Authorization;
using Core.Domain.Authorization;
using Core.Domain.Entities;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class AccessProfileTests
{
    [Fact]
    public void Grant_ShouldAddPermission_WhenCodeIsValid()
    {
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, []).Value;

        var result = profile.Grant(Permissions.TutorsRead);

        result.IsSuccess.Should().BeTrue();
        profile.PermissionCodes.Should().Contain(Permissions.TutorsRead);
    }

    [Fact]
    public void Grant_ShouldFail_WhenCodeIsInvalid()
    {
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, []).Value;

        var result = profile.Grant("Invalid.Permission");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AccessProfile.InvalidPermission");
    }

    [Fact]
    public void MarkForDeletion_ShouldFail_WhenSystemProfile()
    {
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, Permissions.All).Value;

        profile.MarkForDeletion().IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Rename_ShouldFail_WhenSystemProfile()
    {
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, []).Value;

        profile.Rename("New Name").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Clone_ShouldCreateCustomProfile_WithSamePermissions()
    {
        var profile = AccessProfile.CreateSystem("Receptionist", ApplicationRoles.Receptionist, Permissions.ReceptionistDefaults()).Value;

        var clone = profile.Clone("Recepção custom");

        clone.IsSuccess.Should().BeTrue();
        clone.Value.IsSystem.Should().BeFalse();
        clone.Value.PermissionCodes.Should().BeEquivalentTo(profile.PermissionCodes);
    }

    [Fact]
    public void CreateCustom_ShouldAllowRenameRevokeAndSetPermissions()
    {
        var created = AccessProfile.CreateCustom("Recepção+", "extra", ApplicationRoles.Receptionist, [Permissions.TutorsRead]);

        created.IsSuccess.Should().BeTrue();
        var profile = created.Value;
        profile.SetDescription("desk");
        profile.Rename("Recepção plus").IsSuccess.Should().BeTrue();
        profile.Name.Should().Be("Recepção plus");
        profile.Revoke(Permissions.TutorsRead).IsSuccess.Should().BeTrue();
        profile.PermissionCodes.Should().BeEmpty();
        profile.SetPermissions([Permissions.TutorsRead, Permissions.PetsRead]).IsSuccess.Should().BeTrue();
        profile.PermissionCodes.Should().BeEquivalentTo([Permissions.TutorsRead, Permissions.PetsRead]);
        profile.MarkForDeletion().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateCustom_WithInvalidName_ReturnsFailure()
    {
        AccessProfile.CreateCustom(" ", null, ApplicationRoles.Admin, []).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetPermissions_WithInvalidCode_ReturnsFailure()
    {
        var profile = AccessProfile.CreateCustom("Custom", null, ApplicationRoles.Admin, [Permissions.TutorsRead]).Value;

        profile.SetPermissions(["Nope.Permission"]).IsFailure.Should().BeTrue();
    }
}
