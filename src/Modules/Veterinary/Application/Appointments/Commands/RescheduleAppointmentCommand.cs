using System;
using Core.Application.Messaging;

namespace Veterinary.Application.Appointments.Commands;

public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTimeOffset NewDate) : ICommand;
