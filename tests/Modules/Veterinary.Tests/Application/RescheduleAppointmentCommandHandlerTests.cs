using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class RescheduleAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly RescheduleAppointmentCommandHandler _handler;

    public RescheduleAppointmentCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _scheduleSlotRepository = Substitute.For<IScheduleSlotRepository>();
        _handler = new RescheduleAppointmentCommandHandler(_appointmentRepository, _scheduleSlotRepository);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldRescheduleAppointment()
    {
        var appointmentId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var oldDate = DateTimeOffset.UtcNow.AddDays(1);
        var newDate = DateTimeOffset.UtcNow.AddDays(2);

        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), vetId, oldDate, 30, "Checkup").Value;

        _appointmentRepository.GetByIdAsync(appointmentId, Arg.Any<CancellationToken>())
            .Returns(appointment);

        var slot = new ScheduleSlot(Guid.NewGuid(), vetId, newDate, newDate.TimeOfDay, newDate.TimeOfDay.Add(TimeSpan.FromMinutes(30)));
        _scheduleSlotRepository.GetAvailableSlotsAsync(vetId, newDate, Arg.Any<CancellationToken>())
            .Returns(new[] { slot });

        var command = new RescheduleAppointmentCommand(appointmentId, newDate);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Date.Should().Be(newDate);
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ShouldReturnFailure()
    {
        var command = new RescheduleAppointmentCommand(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(2));

        _appointmentRepository.GetByIdAsync(command.AppointmentId, Arg.Any<CancellationToken>())
            .Returns((Appointment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }
}
