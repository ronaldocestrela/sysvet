using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>Online ward unit configuration (pull-only in sync; writes go direct to API).</summary>
public interface IWardUnitApiService
{
    Task<Result<IReadOnlyList<WardUnitListItemDto>>> ListAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(string name, IReadOnlyList<WardBedInputDto> beds, CancellationToken cancellationToken = default);
}

public sealed class WardUnitListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int BedCount { get; init; }
}

public sealed class WardBedInputDto
{
    public Guid? BedId { get; init; }
    public string Code { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class CreateWardUnitRequestDto
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<WardBedInputDto> Beds { get; init; } = Array.Empty<WardBedInputDto>();
}
