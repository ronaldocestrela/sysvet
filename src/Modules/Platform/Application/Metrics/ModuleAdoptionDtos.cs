using Core.Domain.Entitlements;

namespace Platform.Application.Metrics;

/// <summary>Module adoption heatmap for Super Admin (10.3).</summary>
public sealed record PlatformModuleAdoptionDto(
    IReadOnlyList<TenantModuleAdoptionRowDto> Tenants,
    IReadOnlyList<ModuleAdoptionSummaryDto> ModuleSummaries);

/// <summary>One tenant row in the adoption matrix.</summary>
public sealed record TenantModuleAdoptionRowDto(
    Guid TenantId,
    string DisplayName,
    IReadOnlyList<ModuleAdoptionCellDto> Cells);

/// <summary>Enabled flag for one module on a tenant.</summary>
public sealed record ModuleAdoptionCellDto(CommercialModule Module, bool Enabled);

/// <summary>Adoption summary for one module.</summary>
public sealed record ModuleAdoptionSummaryDto(CommercialModule Module, int EnabledCount, decimal AdoptionRate);

/// <summary>CSV export payload for adoption matrix.</summary>
public sealed record ModuleAdoptionFileDto(byte[] Content, string FileName, string ContentType);
