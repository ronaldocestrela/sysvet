using Core.Application.Messaging;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Development-only registration of a user with a clinic role (guarded in handler and API).
/// </summary>
public sealed record RegisterUserCommand(string Email, string Password, string Role, Guid? TenantId = null) : ICommand<Guid>;
