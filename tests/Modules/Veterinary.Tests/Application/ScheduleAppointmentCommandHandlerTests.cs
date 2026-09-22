using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Appointments;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class ScheduleAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly ScheduleAppointmentCommandHandler _handler;

    public ScheduleAppointmentCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _scheduleSlotRepository = Substitute.For<IScheduleSlotRepository>();
        var scheduler = new AppointmentScheduler(_appointmentRepository, _scheduleSlotRepository);
        _handler = new ScheduleAppointmentCommandHandler(scheduler);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_SlotIsAvailable()
    {
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1).Date.Add(TimeSpan.FromHours(10)),
            30,
            "Checkup");

        var slot = new ScheduleSlot(Guid.NewGuid(), command.VeterinarianId, command.Date.Date, TimeSpan.FromHours(10), TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30)));

        _scheduleSlotRepository.GetAvailableSlotsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<ScheduleSlot> { slot });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        slot.IsAvailable.Should().BeFalse();
        await _appointmentRepository.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_SlotIsNotAvailable()
    {
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1).Date.Add(TimeSpan.FromHours(10)),
            30,
            "Checkup");

        _scheduleSlotRepository.GetAvailableSlotsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<ScheduleSlot>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Appointment.SlotUnavailable");
        await _appointmentRepository.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
    }
}
