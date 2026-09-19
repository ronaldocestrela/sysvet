using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Application.Users.Commands;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Core.Tests.Application.Users;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProfileExists_CreatesUser()
    {
        var tenantId = Guid.NewGuid();
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, Permissions.All).Value;
        var identity = Substitute.For<IIdentityService>();
        var profiles = Substitute.For<IAccessProfileRepository>();
        var seeder = Substitute.For<IAccessProfileSeeder>();
        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns(tenantId);
        profiles.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        identity.CreateUserAsync(
            "staff@sysvet.com",
            "Password123!",
            ApplicationRoles.Admin,
            tenantId,
            profile.Id,
            "Staff",
            Arg.Any<CancellationToken>()).Returns(Result.Success("user-id"));

        var handler = new CreateUserCommandHandler(identity, profiles, seeder, tenant);
        var result = await handler.Handle(
            new CreateUserCommand("staff@sysvet.com", "Password123!", profile.Id, "Staff"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("user-id");
        await seeder.Received(1).EnsureTenantProfilesAsync(tenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTenantMissing_ReturnsForbidden()
    {
        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns(Guid.Empty);
        var handler = new CreateUserCommandHandler(
            Substitute.For<IIdentityService>(),
            Substitute.For<IAccessProfileRepository>(),
            Substitute.For<IAccessProfileSeeder>(),
            tenant);

        var result = await handler.Handle(
            new CreateUserCommand("a@b.com", "Password123!", Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Authorization.Forbidden");
    }

    [Fact]
    public async Task Handle_WhenProfileMissing_ReturnsNotFound()
    {
        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns(Guid.NewGuid());
        var profiles = Substitute.For<IAccessProfileRepository>();
        profiles.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AccessProfile?)null);

        var handler = new CreateUserCommandHandler(
            Substitute.For<IIdentityService>(),
            profiles,
            Substitute.For<IAccessProfileSeeder>(),
            tenant);

        var result = await handler.Handle(
            new CreateUserCommand("a@b.com", "Password123!", Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserAccount.ProfileNotFound");
    }
}
