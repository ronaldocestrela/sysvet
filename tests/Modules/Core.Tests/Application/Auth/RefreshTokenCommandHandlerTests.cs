using Core.Application.Auth.Commands;
using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IAccessTokenIssuer _accessTokenIssuer;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenStore = Substitute.For<IRefreshTokenStore>();
        _accessTokenIssuer = Substitute.For<IAccessTokenIssuer>();
        _accessTokenIssuer.AccessTokenLifetimeSeconds.Returns(3600);
        _handler = new RefreshTokenCommandHandler(_refreshTokenStore, _accessTokenIssuer);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailure()
    {
        _refreshTokenStore.RotateAsync("bad", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticatedUserDto>(ErrorCodes.Auth.InvalidRefreshToken));

        var result = await _handler.Handle(new RefreshTokenCommand("bad"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var user = new AuthenticatedUserDto("user-1", "a@b.com", Guid.NewGuid(), Guid.NewGuid(), ["Admin"]);
        _refreshTokenStore.RotateAsync("old-refresh", Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        _accessTokenIssuer.IssueAccessToken(user).Returns("new-access");
        _refreshTokenStore.IssueAsync("user-1", Arg.Any<CancellationToken>()).Returns("new-refresh");

        var result = await _handler.Handle(new RefreshTokenCommand("old-refresh"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-refresh");
    }
}
