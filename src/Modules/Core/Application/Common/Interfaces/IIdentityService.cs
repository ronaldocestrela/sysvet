using Core.Application.Auth.Dtos;
using Core.Domain;

namespace Core.Application.Common.Interfaces;

/// <summary>
/// Abstracts ASP.NET Core Identity for use cases without referencing infrastructure types.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Validates email and password, applying lockout rules on failure.
    /// </summary>
    Task<Result<AuthenticatedUserDto>> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a user with the given role and optional tenant.
    /// </summary>
    Task<Result<string>> CreateUserAsync(string email, string password, string role, Guid tenantId, CancellationToken cancellationToken = default);
}
