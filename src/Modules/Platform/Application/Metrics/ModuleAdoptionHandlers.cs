using Core.Application.Caching;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Services;

namespace Platform.Application.Metrics;

/// <summary>Loads module adoption heatmap (10.3).</summary>
public sealed record GetPlatformModuleAdoptionQuery : IRequest<Result<PlatformModuleAdoptionDto>>, IPlatformScopedCacheQuery
{
    /// <inheritdoc />
    public string CacheKeySuffix => "snapshot";

    /// <inheritdoc />
    public TimeSpan CacheDuration => CacheDurations.Analytics;
}

/// <summary>Exports module adoption matrix as CSV (10.3).</summary>
public sealed record ExportPlatformModuleAdoptionQuery : IRequest<Result<ModuleAdoptionFileDto>>;

/// <summary>Handles module adoption queries.</summary>
public sealed class GetPlatformModuleAdoptionQueryHandler : IRequestHandler<GetPlatformModuleAdoptionQuery, Result<PlatformModuleAdoptionDto>>
{
    private readonly IModuleAdoptionReader _reader;

    /// <summary>Creates the handler.</summary>
    public GetPlatformModuleAdoptionQueryHandler(IModuleAdoptionReader reader) => _reader = reader;

    /// <inheritdoc />
    public async Task<Result<PlatformModuleAdoptionDto>> Handle(
        GetPlatformModuleAdoptionQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await _reader.GetSnapshotAsync(cancellationToken);
        return Result.Success(Map(snapshot));
    }

    internal static PlatformModuleAdoptionDto Map(ModuleAdoptionSnapshot snapshot) =>
        new(
            snapshot.Tenants.Select(t => new TenantModuleAdoptionRowDto(
                t.TenantId,
                t.DisplayName,
                t.Modules.Select(m => new ModuleAdoptionCellDto(m.Key, m.Value)).ToList())).ToList(),
            snapshot.ModuleSummaries.Select(s => new ModuleAdoptionSummaryDto(s.Module, s.EnabledCount, s.AdoptionRate)).ToList());
}

/// <summary>Handles module adoption CSV export.</summary>
public sealed class ExportPlatformModuleAdoptionQueryHandler : IRequestHandler<ExportPlatformModuleAdoptionQuery, Result<ModuleAdoptionFileDto>>
{
    private readonly IModuleAdoptionReader _reader;

    /// <summary>Creates the handler.</summary>
    public ExportPlatformModuleAdoptionQueryHandler(IModuleAdoptionReader reader) => _reader = reader;

    /// <inheritdoc />
    public async Task<Result<ModuleAdoptionFileDto>> Handle(
        ExportPlatformModuleAdoptionQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await _reader.GetSnapshotAsync(cancellationToken);
        var dto = GetPlatformModuleAdoptionQueryHandler.Map(snapshot);
        return Result.Success(new ModuleAdoptionFileDto(
            ModuleAdoptionCsvExporter.Export(dto),
            "adocao-modulos.csv",
            "text/csv"));
    }
}
