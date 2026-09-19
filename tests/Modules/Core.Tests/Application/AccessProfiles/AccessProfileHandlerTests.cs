using Core.Application.AccessProfiles;
using Core.Application.AccessProfiles.Commands;
using Core.Application.AccessProfiles.Queries;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Core.Tests.Application.AccessProfiles;

public class AccessProfileHandlerTests
{
    [Fact]
    public async Task List_MapsPagedDtos()
    {
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, [Permissions.TutorsRead]).Value;
        var repo = Substitute.For<IAccessProfileRepository>();
        repo.SearchAsync(1, 20, null, Arg.Any<CancellationToken>())
            .Returns(new PagedList<AccessProfile>([profile], 1));

        var handler = new ListAccessProfilesQueryHandler(repo);
        var result = await handler.Handle(new ListAccessProfilesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle(d => d.Name == "Admin" && d.IsSystem);
        AccessProfileMappings.ToDto(profile).PermissionCodes.Should().Contain(Permissions.TutorsRead);
    }

    [Fact]
    public async Task Create_ClonesSourceProfile()
    {
        var source = AccessProfile.CreateSystem("Receptionist", ApplicationRoles.Receptionist, Permissions.ReceptionistDefaults()).Value;
        var repo = Substitute.For<IAccessProfileRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        repo.GetByNameAsync("Custom desk", Arg.Any<CancellationToken>()).Returns((AccessProfile?)null);
        repo.GetByIdAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);

        var handler = new CreateAccessProfileCommandHandler(repo, uow);
        var result = await handler.Handle(
            new CreateAccessProfileCommand("Custom desk", source.Id, "cloned"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Received(1).Add(Arg.Any<AccessProfile>());
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenCustomAndUnused_RemovesProfile()
    {
        var profile = AccessProfile.CreateCustom("Extra", null, ApplicationRoles.Cashier, [Permissions.TutorsRead]).Value;
        var repo = Substitute.For<IAccessProfileRepository>();
        var identity = Substitute.For<IIdentityService>();
        var uow = Substitute.For<IUnitOfWork>();
        repo.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);
        identity.CountUsersWithProfileAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(0);

        var handler = new DeleteAccessProfileCommandHandler(repo, identity, uow);
        var result = await handler.Handle(new DeleteAccessProfileCommand(profile.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Received(1).Remove(profile);
    }

    [Fact]
    public async Task Delete_WhenSystem_ReturnsFailure()
    {
        var profile = AccessProfile.CreateSystem("Admin", ApplicationRoles.Admin, Permissions.All).Value;
        var repo = Substitute.For<IAccessProfileRepository>();
        repo.GetByIdAsync(profile.Id, Arg.Any<CancellationToken>()).Returns(profile);

        var handler = new DeleteAccessProfileCommandHandler(
            repo,
            Substitute.For<IIdentityService>(),
            Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new DeleteAccessProfileCommand(profile.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AccessProfile.CannotDeleteSystem");
    }
}
