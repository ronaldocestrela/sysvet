using Core.Application.Auth.Dtos;
using Core.Application.Messaging;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Rotates refresh tokens for tutor portal users.
/// </summary>
public sealed record RefreshTutorTokenCommand(string RefreshToken) : ICommand<AuthTokensDto>;
