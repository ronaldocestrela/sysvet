namespace ClinicSite.Application.Dtos;

/// <summary>
/// Staff view of the clinic site configuration.
/// </summary>
public sealed record ClinicSiteStaffDto(
    Guid Id,
    string DisplayName,
    string? Tagline,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string PostalCode,
    string Phone,
    string Email,
    string? WhatsApp,
    string? LogoUrl,
    string Slug,
    bool IsPublished,
    IReadOnlyList<ClinicSiteServiceDto> Services,
    IReadOnlyList<ClinicSiteTeamMemberDto> Team,
    IReadOnlyList<ClinicSiteHoursDto> Hours);

/// <summary>
/// Public anonymous payload for the marketing site.
/// </summary>
public sealed record PublicClinicSiteDto(
    string Slug,
    string DisplayName,
    string? Tagline,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string PostalCode,
    string Phone,
    string Email,
    string? WhatsApp,
    string? LogoUrl,
    IReadOnlyList<ClinicSiteServiceDto> Services,
    IReadOnlyList<ClinicSiteTeamMemberDto> Team,
    IReadOnlyList<ClinicSiteHoursDto> Hours);

/// <summary>
/// Service line on the public site.
/// </summary>
public sealed record ClinicSiteServiceDto(
    Guid Id,
    string Name,
    string? Description,
    int? DurationMinutes,
    decimal? Price,
    int SortOrder,
    bool IsVisible);

/// <summary>
/// Team member on the public site.
/// </summary>
public sealed record ClinicSiteTeamMemberDto(
    Guid Id,
    string Name,
    string RoleTitle,
    string? Bio,
    int SortOrder,
    bool IsVisible);

/// <summary>
/// Opening hours row on the public site.
/// </summary>
public sealed record ClinicSiteHoursDto(
    Guid Id,
    DayOfWeek Day,
    string? OpenTime,
    string? CloseTime,
    bool IsClosed);
