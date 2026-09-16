using Core.Application.AccessProfiles.Dtos;
using Core.Domain.Entities;

namespace Core.Application.AccessProfiles;

/// <summary>
/// Maps access profile aggregates to API DTOs.
/// </summary>
public static class AccessProfileMappings
{
    /// <summary>
    /// Converts an aggregate to a DTO.
    /// </summary>
    public static AccessProfileDto ToDto(AccessProfile profile) =>
        new(
            profile.Id,
            profile.Name,
            profile.Description,
            profile.IsSystem,
            profile.BaseRole,
            profile.PermissionCodes.OrderBy(c => c, StringComparer.Ordinal).ToList());
}
