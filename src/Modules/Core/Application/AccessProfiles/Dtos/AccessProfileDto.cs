namespace Core.Application.AccessProfiles.Dtos;

/// <summary>
/// Access profile with permission matrix for admin APIs.
/// </summary>
public sealed record AccessProfileDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    string BaseRole,
    IReadOnlyList<string> PermissionCodes);
