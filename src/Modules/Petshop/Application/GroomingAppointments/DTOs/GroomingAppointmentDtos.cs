using Petshop.Domain.Enums;

namespace Petshop.Application.GroomingAppointments.DTOs;

/// <summary>Appointment row for API and clients.</summary>
public sealed record GroomingAppointmentDto(
    Guid Id,
    Guid TutorId,
    Guid PetId,
    Guid GroomerId,
    Guid GroomingServiceId,
    DateTimeOffset Date,
    int DurationInMinutes,
    string Notes,
    GroomingAppointmentStatus Status);
