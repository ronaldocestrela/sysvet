using FluentAssertions;
using NSubstitute;
using Veterinary.Application.MedicalRecords.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class CreateMedicalRecordCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointmentRepository.GetByIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns(appointment);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository);
        var command = new CreateMedicalRecordCommand(appointmentId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await medicalRecordRepository.Received(1).AddAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidAppointment_ReturnsFailure()
    {
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository);
        var command = new CreateMedicalRecordCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }
}
