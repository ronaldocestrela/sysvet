namespace Core.Application.Users.Dtos;

/// <summary>
/// Staff user exposed by admin APIs (Identity-backed).
/// </summary>
public sealed record StaffUserDto(
    string Id,
    string Email,
    string? DisplayName,
    Guid TenantId,
    Guid AccessProfileId,
    string AccessProfileName,
    bool IsDisabled,
    IReadOnlyList<string> Roles);
