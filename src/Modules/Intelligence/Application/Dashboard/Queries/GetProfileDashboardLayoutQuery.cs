using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Intelligence.Application.Dashboard.Dtos;

namespace Intelligence.Application.Dashboard.Queries;

/// <summary>Loads effective or persisted dashboard layout for an access profile.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record GetProfileDashboardLayoutQuery(Guid AccessProfileId) : IQuery<ProfileDashboardLayoutDto>;
