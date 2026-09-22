using Core.Domain;
using TutorPortal.Application.Scheduling;
using TutorPortal.Application.Scheduling.Dtos;

namespace TutorPortal.Application.Abstractions;

/// <summary>
/// Cross-module read/write port for tutor self-service scheduling (Veterinary + Petshop).
/// </summary>
public interface ITutorSchedulingPort
{
    /// <summary>Lists clinical consultation and active grooming services.</summary>
    Task<IReadOnlyList<TutorBookableServiceDto>> ListServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists professionals with schedule slots on the given calendar day.</summary>
    Task<IReadOnlyList<TutorBookingProfessionalDto>> ListProfessionalsAsync(
        TutorBookingKind kind,
        Guid serviceId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default);

    /// <summary>Lists bookable start times for a professional on a day.</summary>
    Task<IReadOnlyList<TutorAvailableSlotDto>> ListAvailableSlotsAsync(
        TutorBookingKind kind,
        Guid serviceId,
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a clinical or grooming appointment for the tutor.</summary>
    Task<Result<Guid>> BookAsync(
        Guid appointmentId,
        Guid tutorId,
        Guid petId,
        TutorBookingKind kind,
        Guid serviceId,
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels a tutor-owned appointment when allowed.</summary>
    Task<Result> CancelAsync(
        Guid tutorId,
        Guid petId,
        TutorBookingKind kind,
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>Lists upcoming and recent cancellable appointments for a pet.</summary>
    Task<IReadOnlyList<TutorPetAppointmentDto>> ListAppointmentsAsync(
        Guid tutorId,
        Guid petId,
        CancellationToken cancellationToken = default);
}
