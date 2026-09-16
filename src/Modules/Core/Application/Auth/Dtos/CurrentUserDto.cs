namespace Core.Application.Auth.Dtos;

/// <summary>
/// Profile of the authenticated caller for the <c>/me</c> endpoint.
/// </summary>
public sealed record CurrentUserDto(
    string Id,
    string Email,
    Guid TenantId,
    IReadOnlyList<string> Roles,
    Guid ProfileId,
    string ProfileName,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Menus);
