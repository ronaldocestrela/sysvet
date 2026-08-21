using Core.Domain;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class ScheduleAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ScheduleAppointmentCommandHandler _handler;

    public ScheduleAppointmentCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _scheduleSlotRepository = Substitute.For<IScheduleSlotRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ScheduleAppointmentCommandHandler(_appointmentRepository, _scheduleSlotRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_SlotIsAvailable()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            DateTimeOffset.UtcNow.AddDays(1).Date.Add(TimeSpan.FromHours(10)), // 10:00 AM tomorrow
            30, 
            "Checkup");

        var slot = new ScheduleSlot(Guid.NewGuid(), command.VeterinarianId, command.Date.Date, TimeSpan.FromHours(10), TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30)));
        
        _scheduleSlotRepository.GetAvailableSlotsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<ScheduleSlot> { slot });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        slot.IsAvailable.Should().BeFalse(); // O slot deve ter sido marcado como não disponível
        await _appointmentRepository.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_SlotIsNotAvailable()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            DateTimeOffset.UtcNow.AddDays(1).Date.Add(TimeSpan.FromHours(10)), // 10:00 AM tomorrow
            30, 
            "Checkup");

        // Nenhum slot disponível
        _scheduleSlotRepository.GetAvailableSlotsAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<ScheduleSlot>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Appointment.SlotUnavailable");
        await _appointmentRepository.DidNotReceive().AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
