using ClinicSite.Application.Dtos;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace ClinicSite.Application.Commands;

/// <summary>
/// Reads the tenant clinic site configuration (creates empty defaults when absent).
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteRead)]
public sealed record GetClinicSiteQuery : IQuery<ClinicSiteStaffDto>;

/// <summary>
/// Updates cadastral and contact fields for the public site.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteWrite)]
public sealed record UpdateClinicSiteProfileCommand(
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
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Replaces service lines shown on the public site.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteWrite)]
public sealed record ReplaceClinicSiteServicesCommand(
    IReadOnlyList<ClinicSiteServiceDto> Services,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Replaces team members shown on the public site.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteWrite)]
public sealed record ReplaceClinicSiteTeamCommand(
    IReadOnlyList<ClinicSiteTeamMemberDto> Team,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Replaces weekly opening hours.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteWrite)]
public sealed record ReplaceClinicSiteHoursCommand(
    IReadOnlyList<ClinicSiteHoursDto> Hours,
    Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Publishes the site at the configured slug.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteWrite)]
public sealed record PublishClinicSiteCommand(Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Removes the site from public access.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.ClinicSiteWrite)]
public sealed record UnpublishClinicSiteCommand(Guid IdempotencyKey = default) : ICommand, IIdempotentCommand;

/// <summary>
/// Anonymous read of a published site by slug (tenant resolved by API filter).
/// </summary>
public sealed record GetPublicClinicSiteQuery(string Slug) : IQuery<PublicClinicSiteDto>;
