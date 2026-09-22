using Core.Application.Auth.Dtos;
using Core.Application.Messaging;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Tutor portal login (role <c>Tutor</c> only).
/// </summary>
public sealed record TutorLoginCommand(string Email, string Password) : ICommand<AuthTokensDto>;
