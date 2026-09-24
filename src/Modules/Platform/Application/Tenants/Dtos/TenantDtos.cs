using Platform.Domain.Entities;

namespace Platform.Application.Tenants.Dtos;

/// <summary>Summary row for tenant catalog listings.</summary>
public sealed record TenantSummaryDto(
    Guid Id,
    string Slug,
    string DisplayName,
    TenantStatus Status,
    ReleaseRing ReleaseRing,
    string SchemaName,
    DateTimeOffset UpdatedAt);

/// <summary>Tenant detail including branch count.</summary>
public sealed record TenantDetailDto(
    Guid Id,
    string Slug,
    string DisplayName,
    TenantStatus Status,
    ReleaseRing ReleaseRing,
    string SchemaName,
    DateTimeOffset UpdatedAt,
    int BranchCount);

/// <summary>Onboarding success payload.</summary>
public sealed record OnboardTenantResultDto(Guid TenantId, string AdminUserId, string Slug);

/// <summary>Branch row for platform API.</summary>
public sealed record BranchDto(
    Guid Id,
    Guid TenantId,
    string Cnpj,
    string LegalName,
    bool IsHeadquarters);
