namespace Core.Application.Auth.Dtos;

/// <summary>
/// Application-layer representation of an Identity user after successful credential validation.
/// </summary>
public sealed record AuthenticatedUserDto(string UserId, string Email, Guid TenantId, IReadOnlyList<string> Roles);
