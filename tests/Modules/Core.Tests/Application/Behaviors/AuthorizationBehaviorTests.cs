using Core.Application.Behaviors;
using Core.Application.Common.Interfaces;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Authorization;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Behaviors;

public class AuthorizationBehaviorTests
{
    private class OpenQuery : IQuery<string>
    {
    }

    [AuthorizeRequest("ClinicStaff")]
    private class ProtectedQuery : IQuery<string>
    {
    }

    [AuthorizeRequest("ClinicStaff", Permissions.VaccinesRead)]
    private class VaccineListQuery : IQuery<IReadOnlyList<string>>
    {
    }

    [Fact]
    public async Task Handle_WithoutAttribute_ShouldInvokeNext()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        var permissionChecker = Substitute.For<IPermissionChecker>();
        var behavior = new AuthorizationBehavior<OpenQuery, Result<string>>(currentUser, permissionChecker);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Success("ok")));

        var result = await behavior.Handle(new OpenQuery(), next, CancellationToken.None);

        result.Value.Should().Be("ok");
        await currentUser.DidNotReceive().IsInPolicyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(false);
        var permissionChecker = Substitute.For<IPermissionChecker>();
        var behavior = new AuthorizationBehavior<ProtectedQuery, Result<string>>(currentUser, permissionChecker);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();

        var result = await behavior.Handle(new ProtectedQuery(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Authorization.Unauthorized);
        await next.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_WhenPolicyFails_ShouldReturnForbidden()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsInPolicyAsync("ClinicStaff", Arg.Any<CancellationToken>()).Returns(false);
        var permissionChecker = Substitute.For<IPermissionChecker>();
        var behavior = new AuthorizationBehavior<ProtectedQuery, Result<string>>(currentUser, permissionChecker);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();

        var result = await behavior.Handle(new ProtectedQuery(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Authorization.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenPermissionDenied_ShouldReturnForbiddenForReadOnlyListResult()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsInPolicyAsync("ClinicStaff", Arg.Any<CancellationToken>()).Returns(true);
        var permissionChecker = Substitute.For<IPermissionChecker>();
        permissionChecker.HasPermissionAsync(Permissions.VaccinesRead, Arg.Any<CancellationToken>()).Returns(false);
        var behavior = new AuthorizationBehavior<VaccineListQuery, Result<IReadOnlyList<string>>>(currentUser, permissionChecker);
        var next = Substitute.For<RequestHandlerDelegate<Result<IReadOnlyList<string>>>>();

        var result = await behavior.Handle(new VaccineListQuery(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Authorization.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenPolicySucceeds_ShouldInvokeNext()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.IsInPolicyAsync("ClinicStaff", Arg.Any<CancellationToken>()).Returns(true);
        var permissionChecker = Substitute.For<IPermissionChecker>();
        var behavior = new AuthorizationBehavior<ProtectedQuery, Result<string>>(currentUser, permissionChecker);
        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        next.Invoke().Returns(Task.FromResult(Result.Success("ok")));

        var result = await behavior.Handle(new ProtectedQuery(), next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
