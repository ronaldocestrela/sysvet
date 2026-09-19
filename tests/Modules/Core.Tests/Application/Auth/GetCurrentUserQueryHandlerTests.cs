using Core.Application.Auth.Queries;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Application.Users.Dtos;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Auth;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(false);
        var tenantContext = Substitute.For<ITenantContext>();
        var identity = Substitute.For<IIdentityService>();
        var permissionChecker = Substitute.For<IPermissionChecker>();
        var accessProfiles = Substitute.For<IAccessProfileRepository>();
        var handler = new GetCurrentUserQueryHandler(currentUser, tenantContext, identity, permissionChecker, accessProfiles);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Authorization.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnProfileWithMenus()
    {
        var tenantId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns("user-abc");
        currentUser.Email.Returns("vet@sysvet.com");
        currentUser.TenantId.Returns(tenantId);
        currentUser.Roles.Returns(new[] { "Veterinarian" });
        var tenantContext = Substitute.For<ITenantContext>();
        var identity = Substitute.For<IIdentityService>();
        identity.GetByIdAsync("user-abc", tenantId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(new StaffUserDto(
                "user-abc",
                "vet@sysvet.com",
                "Dr Vet",
                tenantId,
                profileId,
                "Veterinarian",
                false,
                ["Veterinarian"])));
        var permissionChecker = Substitute.For<IPermissionChecker>();
        permissionChecker.GetGrantedPermissionsAsync(Arg.Any<CancellationToken>())
            .Returns(Permissions.VeterinarianDefaults());
        var accessProfiles = Substitute.For<IAccessProfileRepository>();
        accessProfiles.GetByIdAsync(profileId, Arg.Any<CancellationToken>())
            .Returns(AccessProfile.CreateSystem("Veterinarian", ApplicationRoles.Veterinarian, Permissions.VeterinarianDefaults()).Value);
        var handler = new GetCurrentUserQueryHandler(currentUser, tenantContext, identity, permissionChecker, accessProfiles);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("user-abc");
        result.Value.ProfileId.Should().Be(profileId);
        result.Value.Menus.Should().Contain("tutors");
    }
}
