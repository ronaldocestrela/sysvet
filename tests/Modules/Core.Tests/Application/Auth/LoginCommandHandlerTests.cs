using Core.Application.Auth.Commands;
using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Auth;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _accessTokenIssuer = Substitute.For<IAccessTokenIssuer>();
        _refreshTokenStore = Substitute.For<IRefreshTokenStore>();
        _accessTokenIssuer.AccessTokenLifetimeSeconds.Returns(3600);
        _handler = new LoginCommandHandler(_identityService, _accessTokenIssuer, _refreshTokenStore);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        _identityService.ValidateCredentialsAsync("a@b.com", "wrong", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidCredentials));

        var result = await _handler.Handle(new LoginCommand("a@b.com", "wrong"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        var user = new AuthenticatedUserDto("user-1", "a@b.com", Guid.NewGuid(), ["Admin"]);
        _identityService.ValidateCredentialsAsync("a@b.com", "Password123!", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _accessTokenIssuer.IssueAccessToken(user).Returns("access-jwt");
        _refreshTokenStore.IssueAsync("user-1", Arg.Any<CancellationToken>()).Returns("refresh-plain");

        var result = await _handler.Handle(new LoginCommand("a@b.com", "Password123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-jwt");
        result.Value.RefreshToken.Should().Be("refresh-plain");
        result.Value.ExpiresInSeconds.Should().Be(3600);
    }
}
