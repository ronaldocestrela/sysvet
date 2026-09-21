using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Client port for grooming salon agenda — offline-first with sync outbox.
/// </summary>
public interface IGroomingStore
{
    Task<Result<List<GroomingAppointmentListItemDto>>> GetDailyAsync(Guid? groomerId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> MarkReadyAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> CompleteAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<GroomingHistoryItemDto>>> GetPetHistoryAsync(Guid petId, CancellationToken cancellationToken = default);
}

public sealed class GroomingAppointmentListItemDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public Guid GroomerId { get; init; }
    public DateTimeOffset Date { get; init; }
    public string Notes { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PetName { get; init; } = string.Empty;
}

public sealed class GroomingHistoryItemDto
{
    public Guid GroomingAppointmentId { get; init; }
    public DateTimeOffset Date { get; init; }
    public string CoatNotes { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
