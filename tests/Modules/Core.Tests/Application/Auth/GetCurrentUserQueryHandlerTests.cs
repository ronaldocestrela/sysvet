using Core.Application.Auth.Queries;
using Core.Application.Common.Interfaces;
using Core.Domain;
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
        var handler = new GetCurrentUserQueryHandler(currentUser, tenantContext);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Authorization.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnProfile()
    {
        var tenantId = Guid.NewGuid();
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns("user-abc");
        currentUser.Email.Returns("vet@sysvet.com");
        currentUser.TenantId.Returns(tenantId);
        currentUser.Roles.Returns(new[] { "Veterinarian" });
        var tenantContext = Substitute.For<ITenantContext>();
        var handler = new GetCurrentUserQueryHandler(currentUser, tenantContext);

        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("user-abc");
        result.Value.Email.Should().Be("vet@sysvet.com");
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Roles.Should().Contain("Veterinarian");
    }
}
