using Core.Domain;

namespace Petshop.Application.GroomingAppointments;

/// <summary>
/// Shared booking and cancellation orchestration for grooming appointments (staff and tutor portal).
/// </summary>
public interface IGroomingAppointmentScheduler
{
    /// <summary>
    /// Books a grooming appointment with slot booking and draft record seeding, or returns existing id.
    /// </summary>
    Task<Result<Guid>> ScheduleAsync(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid groomerId,
        Guid groomingServiceId,
        DateTimeOffset date,
        int durationInMinutes,
        string? notes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the grooming appointment and releases the covering slot when booked.
    /// </summary>
    Task<Result> CancelAndReleaseSlotAsync(Guid groomingAppointmentId, CancellationToken cancellationToken = default);
}
