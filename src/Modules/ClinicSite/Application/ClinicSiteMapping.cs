using ClinicSite.Application.Dtos;
using ClinicSite.Domain.Entities;

namespace ClinicSite.Application;

/// <summary>
/// Maps domain entities to clinic site DTOs.
/// </summary>
public static class ClinicSiteMapping
{
    /// <summary>
    /// Maps the staff aggregate to a configuration DTO.
    /// </summary>
    public static ClinicSiteStaffDto ToStaffDto(ClinicSiteProfile profile) =>
        new(
            profile.Id,
            profile.DisplayName,
            profile.Tagline,
            profile.Street,
            profile.Number,
            profile.Complement,
            profile.District,
            profile.City,
            profile.State,
            profile.PostalCode,
            profile.Phone,
            profile.Email,
            profile.WhatsApp,
            profile.LogoUrl,
            profile.Slug,
            profile.IsPublished,
            profile.Services.Select(ToServiceDto).OrderBy(s => s.SortOrder).ToList(),
            profile.Team.Select(ToTeamDto).OrderBy(t => t.SortOrder).ToList(),
            profile.Hours.Select(ToHoursDto).OrderBy(h => h.Day).ToList());

    /// <summary>
    /// Maps a published profile to the anonymous public DTO (visible rows only).
    /// </summary>
    public static PublicClinicSiteDto ToPublicDto(ClinicSiteProfile profile) =>
        new(
            profile.Slug,
            profile.DisplayName,
            profile.Tagline,
            profile.Street,
            profile.Number,
            profile.Complement,
            profile.District,
            profile.City,
            profile.State,
            profile.PostalCode,
            profile.Phone,
            profile.Email,
            profile.WhatsApp,
            profile.LogoUrl,
            profile.Services.Where(s => s.IsVisible).Select(ToServiceDto).OrderBy(s => s.SortOrder).ToList(),
            profile.Team.Where(t => t.IsVisible).Select(ToTeamDto).OrderBy(t => t.SortOrder).ToList(),
            profile.Hours.Select(ToHoursDto).OrderBy(h => h.Day).ToList());

    private static ClinicSiteServiceDto ToServiceDto(ClinicSiteServiceItem item) =>
        new(item.Id, item.Name, item.Description, item.DurationMinutes, item.Price, item.SortOrder, item.IsVisible);

    private static ClinicSiteTeamMemberDto ToTeamDto(ClinicSiteTeamMember item) =>
        new(item.Id, item.Name, item.RoleTitle, item.Bio, item.SortOrder, item.IsVisible);

    private static ClinicSiteHoursDto ToHoursDto(ClinicSiteOpeningHours item) =>
        new(
            item.Id,
            item.Day,
            item.OpenTime?.ToString("HH:mm"),
            item.CloseTime?.ToString("HH:mm"),
            item.IsClosed);
}
