using Core.Domain;

namespace Veterinary.Application.Appointments;

/// <summary>
/// Shared booking and cancellation orchestration for clinical appointments (staff and tutor portal).
/// </summary>
public interface IAppointmentScheduler
{
    /// <summary>
    /// Books an appointment when a covering slot is available, or returns the existing id when already scheduled.
    /// </summary>
    Task<Result<Guid>> ScheduleAsync(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid veterinarianId,
        DateTimeOffset date,
        int durationInMinutes,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the appointment and releases the covering schedule slot when booked.
    /// </summary>
    Task<Result> CancelAndReleaseSlotAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
