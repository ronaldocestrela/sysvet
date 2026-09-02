using System;
using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Xunit;
using Core.Domain;

namespace Veterinary.Tests.Domain;

public class AppointmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAppointmentInScheduledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var veterinarianId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow.AddDays(1); // Future date
        var durationInMinutes = 30;
        var reason = "Routine checkup";

        // Act
        var appointment = Appointment.Create(id, tutorId, petId, veterinarianId, date, durationInMinutes, reason);

        // Assert
        appointment.IsSuccess.Should().BeTrue();
        appointment.Value.Id.Should().Be(id);
        appointment.Value.TutorId.Should().Be(tutorId);
        appointment.Value.PetId.Should().Be(petId);
        appointment.Value.VeterinarianId.Should().Be(veterinarianId);
        appointment.Value.Date.Should().Be(date);
        appointment.Value.DurationInMinutes.Should().Be(durationInMinutes);
        appointment.Value.Reason.Should().Be(reason);
        appointment.Value.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Create_WithPastDate_ShouldReturnFailure()
    {
        // Arrange
        var pastDate = DateTimeOffset.UtcNow.AddMinutes(-5);

        // Act
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), pastDate, 30, "Checkup");

        // Assert
        appointment.IsSuccess.Should().BeFalse();
        appointment.Error.Code.Should().Be("Appointment.InvalidDate");
    }

    [Fact]
    public void Confirm_WhenScheduled_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;

        // Act
        var result = appointment.Confirm();

        // Assert
        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenCancelled_ShouldReturnFailure()
    {
        // Arrange
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointment.Cancel();

        // Act
        var result = appointment.Confirm();

        // Assert
        result.IsSuccess.Should().BeFalse();
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        result.Error.Code.Should().Be("Appointment.InvalidStatusTransition");
    }

    [Fact]
    public void Reschedule_WithValidNewDate_ShouldUpdateDateAndSetStatusToScheduled()
    {
        // Arrange
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Checkup").Value;
        appointment.Confirm(); // Confirm it first

        var newDate = DateTimeOffset.UtcNow.AddDays(2);

        // Act
        var result = appointment.Reschedule(newDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        appointment.Date.Should().Be(newDate);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled); // Status is reset to scheduled because the date changed
    }
}
