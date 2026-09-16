using Core.Application.Auth.Dtos;
using Core.Application.Messaging;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Authenticates a clinic user with email and password.
/// </summary>
public sealed record LoginCommand(string Email, string Password) : ICommand<AuthTokensDto>;
