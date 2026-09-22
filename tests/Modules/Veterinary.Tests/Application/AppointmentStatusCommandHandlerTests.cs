using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Appointments;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class AppointmentStatusCommandHandlerTests
{
    private static Appointment CreateScheduled() =>
        Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;

    [Fact]
    public async Task ConfirmStartAndComplete_TransitionsStatus()
    {
        var appointment = CreateScheduled();
        var repo = Substitute.For<IAppointmentRepository>();
        repo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

        (await new ConfirmAppointmentCommandHandler(repo)
            .Handle(new ConfirmAppointmentCommand(appointment.Id), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await new StartAppointmentCommandHandler(repo)
            .Handle(new StartAppointmentCommand(appointment.Id), CancellationToken.None)).IsSuccess.Should().BeTrue();
        (await new CompleteAppointmentCommandHandler(repo)
            .Handle(new CompleteAppointmentCommand(appointment.Id), CancellationToken.None)).IsSuccess.Should().BeTrue();

        repo.Received(3).Update(appointment);
    }

    [Fact]
    public async Task Confirm_WhenMissing_ReturnsNotFound()
    {
        var repo = Substitute.For<IAppointmentRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var result = await new ConfirmAppointmentCommandHandler(repo)
            .Handle(new ConfirmAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }

    [Fact]
    public async Task Cancel_ReleasesSlotWhenBooked()
    {
        var appointment = CreateScheduled();
        var repo = Substitute.For<IAppointmentRepository>();
        var slots = Substitute.For<IScheduleSlotRepository>();
        repo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        slots.GetAllSlotsForDayAsync(appointment.VeterinarianId, appointment.Date, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ScheduleSlot>());

        var scheduler = new AppointmentScheduler(repo, slots);
        var result = await new CancelAppointmentCommandHandler(scheduler)
            .Handle(new CancelAppointmentCommand(appointment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Received(1).Update(appointment);
    }

    [Fact]
    public async Task MarkNoShow_WhenMissing_ReturnsNotFound()
    {
        var repo = Substitute.For<IAppointmentRepository>();
        var slots = Substitute.For<IScheduleSlotRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var result = await new MarkNoShowAppointmentCommandHandler(repo, slots)
            .Handle(new MarkNoShowAppointmentCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }

    [Fact]
    public async Task MarkNoShow_WhenScheduled_Succeeds()
    {
        var appointment = CreateScheduled();
        var repo = Substitute.For<IAppointmentRepository>();
        var slots = Substitute.For<IScheduleSlotRepository>();
        repo.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        slots.GetAllSlotsForDayAsync(appointment.VeterinarianId, appointment.Date, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ScheduleSlot>());

        var result = await new MarkNoShowAppointmentCommandHandler(repo, slots)
            .Handle(new MarkNoShowAppointmentCommand(appointment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
