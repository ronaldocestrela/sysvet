using FluentValidation;

namespace Veterinary.Application.Appointments.Commands;

public class ScheduleAppointmentCommandValidator : AbstractValidator<ScheduleAppointmentCommand>
{
    public ScheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.TutorId).NotEmpty();
        RuleFor(x => x.PetId).NotEmpty();
        RuleFor(x => x.VeterinarianId).NotEmpty();
        RuleFor(x => x.Date).GreaterThan(DateTimeOffset.Now);
        RuleFor(x => x.DurationInMinutes).GreaterThan(0);
    }
}
