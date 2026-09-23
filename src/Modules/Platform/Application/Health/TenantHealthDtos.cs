namespace Platform.Application.Health;

/// <summary>Per-module row count slice.</summary>
public sealed record TenantDataVolumeSliceDto(string Module, long RowCount);

/// <summary>Health dashboard payload for a tenant.</summary>
public sealed record TenantHealthDto(
    Guid TenantId,
    IReadOnlyList<TenantDataVolumeSliceDto> DataVolume,
    long TotalRowCount,
    long EstimatedBytes,
    int RequestsToday,
    int RequestsLast7Days,
    DateTimeOffset GeneratedAtUtc);
