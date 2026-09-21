using FluentAssertions;
using Petshop.Domain.Entities;
using Petshop.Domain.Enums;
using Petshop.Domain.Events;
using Xunit;

namespace Petshop.Tests.Domain;

public class GroomingAppointmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldBeScheduled()
    {
        var date = DateTimeOffset.UtcNow.AddDays(1);
        var result = GroomingAppointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), date, 45, "Banho");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(GroomingAppointmentStatus.Scheduled);
    }

    [Fact]
    public void Start_WhenConfirmed_ShouldRaiseGroomingStarted()
    {
        var appointment = GroomingAppointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1), 30, "").Value;
        appointment.Confirm();

        var result = appointment.Start();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(GroomingAppointmentStatus.InProgress);
        appointment.DomainEvents.Should().ContainSingle(e => e is GroomingStartedDomainEvent);
    }

    [Fact]
    public void Complete_WhenInProgress_ShouldRaiseGroomingCompleted()
    {
        var appointment = GroomingAppointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1), 30, "").Value;
        appointment.Confirm();
        appointment.Start();

        var result = appointment.Complete();

        result.IsSuccess.Should().BeTrue();
        appointment.DomainEvents.OfType<GroomingCompletedDomainEvent>().Should().ContainSingle();
    }
}
