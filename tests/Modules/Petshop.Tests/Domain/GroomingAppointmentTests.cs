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
    public void MarkReady_WhenInProgress_ShouldRaiseGroomingReadyForPickup()
    {
        var appointment = CreateInProgress();

        var result = appointment.MarkReady();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(GroomingAppointmentStatus.ReadyForPickup);
        appointment.DomainEvents.Should().ContainSingle(e => e is GroomingReadyForPickupDomainEvent);
    }

    [Fact]
    public void MarkReady_WhenAlreadyReady_ShouldBeIdempotent()
    {
        var appointment = CreateInProgress();
        appointment.MarkReady();
        appointment.ClearDomainEvents();

        var result = appointment.MarkReady();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(GroomingAppointmentStatus.ReadyForPickup);
        appointment.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Complete_WhenInProgress_ShouldRaiseReadyAndCompleted()
    {
        var appointment = CreateInProgress();

        var result = appointment.Complete();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(GroomingAppointmentStatus.Completed);
        appointment.DomainEvents.OfType<GroomingReadyForPickupDomainEvent>().Should().ContainSingle();
        appointment.DomainEvents.OfType<GroomingCompletedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Complete_WhenReadyForPickup_ShouldRaiseOnlyCompleted()
    {
        var appointment = CreateInProgress();
        appointment.MarkReady();
        appointment.ClearDomainEvents();

        var result = appointment.Complete();

        result.IsSuccess.Should().BeTrue();
        appointment.DomainEvents.OfType<GroomingReadyForPickupDomainEvent>().Should().BeEmpty();
        appointment.DomainEvents.OfType<GroomingCompletedDomainEvent>().Should().ContainSingle();
    }

    private static GroomingAppointment CreateInProgress()
    {
        var appointment = GroomingAppointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1), 30, "").Value;
        appointment.Confirm();
        appointment.Start();
        appointment.ClearDomainEvents();
        return appointment;
    }
}
