using Core.Domain;

namespace Clients.Infrastructure.ClinicSite;

/// <summary>
/// Online API client for staff clinic site configuration (Fase 8.7).
/// </summary>
public sealed class ClinicSiteApiService
{
    private readonly Http.ApiClient _apiClient;

    /// <summary>
    /// Creates the clinic site API client.
    /// </summary>
    public ClinicSiteApiService(Http.ApiClient apiClient) => _apiClient = apiClient;

    /// <summary>Loads the tenant site configuration.</summary>
    public Task<Result<ClinicSiteStaffClientDto>> GetAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<ClinicSiteStaffClientDto>("/api/v1/clinic-site", cancellationToken);

    /// <summary>Updates profile and slug.</summary>
    public Task<Result> UpdateProfileAsync(ClinicSiteProfileUpdateClientDto dto, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync("/api/v1/clinic-site", dto, idempotencyKey: null, cancellationToken);

    /// <summary>Replaces service lines.</summary>
    public Task<Result> ReplaceServicesAsync(IReadOnlyList<ClinicSiteServiceClientDto> services, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync("/api/v1/clinic-site/services", new { Services = services }, idempotencyKey: null, cancellationToken);

    /// <summary>Replaces team members.</summary>
    public Task<Result> ReplaceTeamAsync(IReadOnlyList<ClinicSiteTeamMemberClientDto> team, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync("/api/v1/clinic-site/team", new { Team = team }, idempotencyKey: null, cancellationToken);

    /// <summary>Replaces opening hours.</summary>
    public Task<Result> ReplaceHoursAsync(IReadOnlyList<ClinicSiteHoursClientDto> hours, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync("/api/v1/clinic-site/hours", new { Hours = hours }, idempotencyKey: null, cancellationToken);

    /// <summary>Publishes the public site.</summary>
    public Task<Result> PublishAsync(CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync("/api/v1/clinic-site/publish", new { }, idempotencyKey: null, cancellationToken);

    /// <summary>Unpublishes the public site.</summary>
    public Task<Result> UnpublishAsync(CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync("/api/v1/clinic-site/unpublish", new { }, idempotencyKey: null, cancellationToken);
}

/// <summary>Staff clinic site DTO.</summary>
public sealed class ClinicSiteStaffClientDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string? Tagline { get; set; }
    public string Street { get; set; } = "";
    public string Number { get; set; } = "";
    public string? Complement { get; set; }
    public string District { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? WhatsApp { get; set; }
    public string? LogoUrl { get; set; }
    public string Slug { get; set; } = "";
    public bool IsPublished { get; set; }
    public IReadOnlyList<ClinicSiteServiceClientDto> Services { get; set; } = [];
    public IReadOnlyList<ClinicSiteTeamMemberClientDto> Team { get; set; } = [];
    public IReadOnlyList<ClinicSiteHoursClientDto> Hours { get; set; } = [];
}

/// <summary>Profile update payload.</summary>
public sealed class ClinicSiteProfileUpdateClientDto
{
    public string DisplayName { get; set; } = "";
    public string? Tagline { get; set; }
    public string Street { get; set; } = "";
    public string Number { get; set; } = "";
    public string? Complement { get; set; }
    public string District { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? WhatsApp { get; set; }
    public string? LogoUrl { get; set; }
    public string Slug { get; set; } = "";
}

public sealed class ClinicSiteServiceClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Price { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

public sealed class ClinicSiteTeamMemberClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string RoleTitle { get; set; } = "";
    public string? Bio { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

public sealed class ClinicSiteHoursClientDto
{
    public Guid Id { get; set; }
    public DayOfWeek Day { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

/// <summary>Anonymous public site payload.</summary>
public sealed class PublicClinicSiteClientDto
{
    public string Slug { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Tagline { get; set; }
    public string Street { get; set; } = "";
    public string Number { get; set; } = "";
    public string? Complement { get; set; }
    public string District { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? WhatsApp { get; set; }
    public string? LogoUrl { get; set; }
    public IReadOnlyList<ClinicSiteServiceClientDto> Services { get; set; } = [];
    public IReadOnlyList<ClinicSiteTeamMemberClientDto> Team { get; set; } = [];
    public IReadOnlyList<ClinicSiteHoursClientDto> Hours { get; set; } = [];
}
