namespace Core.Application.Sync;

/// <summary>
/// Supplies module rows for sync pull (appointments, slots, etc.).
/// </summary>
public interface ISyncChangeFeedContributor
{
    /// <summary>Reads changes after <paramref name="since"/> up to <paramref name="take"/>.</summary>
    Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken);
}

/// <summary>Partial pull page from one module contributor.</summary>
public sealed class SyncContributorChanges
{
    public IReadOnlyList<SyncAppointmentDto> Appointments { get; init; } = Array.Empty<SyncAppointmentDto>();
    public IReadOnlyList<SyncScheduleSlotDto> ScheduleSlots { get; init; } = Array.Empty<SyncScheduleSlotDto>();
    public DateTimeOffset MaxUpdatedAt { get; init; }
    public bool HasMore { get; init; }
}
