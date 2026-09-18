using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using NSubstitute;
using Veterinary.Application.MedicalRecords.Commands;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class CreateMedicalRecordCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithEligibleAppointment_ReturnsSuccessResult()
    {
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.UserId.Returns(Guid.NewGuid());

        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointment.Confirm();
        appointment.Start();
        appointmentRepository.GetByIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns(appointment);
        medicalRecordRepository.GetByAppointmentIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns((MedicalRecord?)null);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository, auditLogger, tenantContext);
        var command = new CreateMedicalRecordCommand(appointmentId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await medicalRecordRepository.Received(1).AddAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
        await auditLogger.Received(1).LogAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), "MedicalRecord", "Create", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRecordExists_ReturnsExistingIdWithoutAdd()
    {
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var tenantContext = Substitute.For<ITenantContext>();

        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointment.Confirm();
        appointment.Start();
        appointmentRepository.GetByIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns(appointment);

        var existingId = Guid.NewGuid();
        var existing = MedicalRecord.Create(existingId, appointmentId, appointment.VeterinarianId, appointment.TutorId, appointment.PetId).Value;
        medicalRecordRepository.GetByAppointmentIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns(existing);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository, auditLogger, tenantContext);
        var result = await handler.Handle(new CreateMedicalRecordCommand(appointmentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingId);
        await medicalRecordRepository.DidNotReceive().AddAsync(Arg.Any<MedicalRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidAppointment_ReturnsFailure()
    {
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var tenantContext = Substitute.For<ITenantContext>();
        appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository, auditLogger, tenantContext);
        var command = new CreateMedicalRecordCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Appointment.NotFound");
    }

    [Fact]
    public async Task Handle_WithScheduledAppointment_ReturnsNotEligible()
    {
        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        var auditLogger = Substitute.For<IAuditLogger>();
        var tenantContext = Substitute.For<ITenantContext>();

        var appointmentId = Guid.NewGuid();
        var appointment = Appointment.Create(appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointmentRepository.GetByIdAsync(appointmentId, Arg.Any<CancellationToken>()).Returns(appointment);

        var handler = new CreateMedicalRecordCommandHandler(appointmentRepository, medicalRecordRepository, auditLogger, tenantContext);
        var result = await handler.Handle(new CreateMedicalRecordCommand(appointmentId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.AppointmentNotEligible");
    }
}
