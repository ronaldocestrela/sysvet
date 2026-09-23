using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Caching;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Authorization;
using Intelligence.Application.Reports.Dtos;
using MediatR;

namespace Intelligence.Application.Reports;

/// <summary>ABC customers report query (10.3).</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record GetAbcCustomersReportQuery(DateOnly From, DateOnly To) : IRequest<Result<AbcCustomerReportDto>>, ICacheableQuery
{
    /// <inheritdoc />
    public string CacheKeySuffix => $"{From:yyyy-MM-dd}:{To:yyyy-MM-dd}";

    /// <inheritdoc />
    public TimeSpan CacheDuration => CacheDurations.Analytics;
}

/// <summary>ABC products report query (10.3).</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record GetAbcProductsReportQuery(DateOnly From, DateOnly To) : IRequest<Result<AbcProductReportDto>>, ICacheableQuery
{
    /// <inheritdoc />
    public string CacheKeySuffix => $"{From:yyyy-MM-dd}:{To:yyyy-MM-dd}";

    /// <inheritdoc />
    public TimeSpan CacheDuration => CacheDurations.Analytics;
}

/// <summary>Staff productivity report query (10.3).</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record GetProductivityReportQuery(DateOnly From, DateOnly To) : IRequest<Result<ProductivityReportDto>>, ICacheableQuery
{
    /// <inheritdoc />
    public string CacheKeySuffix => $"{From:yyyy-MM-dd}:{To:yyyy-MM-dd}";

    /// <inheritdoc />
    public TimeSpan CacheDuration => CacheDurations.Analytics;
}

/// <summary>CSV export for ABC customers.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record ExportAbcCustomersReportQuery(DateOnly From, DateOnly To) : IRequest<Result<IntelligenceReportFileDto>>;

/// <summary>CSV export for ABC products.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record ExportAbcProductsReportQuery(DateOnly From, DateOnly To) : IRequest<Result<IntelligenceReportFileDto>>;

/// <summary>CSV export for productivity report.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.IntelligenceRead)]
public sealed record ExportProductivityReportQuery(DateOnly From, DateOnly To) : IRequest<Result<IntelligenceReportFileDto>>;
