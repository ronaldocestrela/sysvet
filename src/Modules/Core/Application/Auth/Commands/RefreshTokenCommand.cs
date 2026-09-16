using Core.Application.Auth.Dtos;
using Core.Application.Messaging;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Exchanges a valid refresh token for a new access/refresh token pair.
/// </summary>
public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokensDto>;
