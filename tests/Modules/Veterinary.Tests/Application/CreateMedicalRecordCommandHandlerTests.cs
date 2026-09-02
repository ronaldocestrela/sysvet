using FluentAssertions;
using NSubstitute;
using Xunit;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using Veterinary.Application.MedicalRecords.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Veterinary.Tests.Application;

public class CreateMedicalRecordCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointmentRepository.GetByIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns(appointment);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository, unitOfWork);
        var command = new CreateMedicalRecordCommand(appointmentId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await medicalRecordRepository.Received(1).AddAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidAppointment_ReturnsFailure()
    {
        // Arrange
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository, unitOfWork);
        var command = new CreateMedicalRecordCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }
}
