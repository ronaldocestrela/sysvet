using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Caching;
using Core.Application.Messaging;
using Intelligence.Application.Dashboard.Dtos;

namespace Intelligence.Application.Dashboard.Queries;

/// <summary>Loads the operational dashboard for the current user's access profile.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Core.Domain.Authorization.Permissions.IntelligenceRead)]
public sealed record GetTenantDashboardQuery : IQuery<TenantDashboardDto>, ICacheableQuery
{
    /// <inheritdoc />
    public string CacheKeySuffix => "today";

    /// <inheritdoc />
    public TimeSpan CacheDuration => CacheDurations.Dashboard;
}
