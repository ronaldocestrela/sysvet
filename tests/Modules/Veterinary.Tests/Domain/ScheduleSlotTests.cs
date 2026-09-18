using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain;
using Xunit;

namespace Veterinary.Tests.Domain;

public class ScheduleSlotTests
{
    [Fact]
    public void Book_WhenAvailable_ShouldMarkUnavailable()
    {
        var slot = new ScheduleSlot(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(9), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)));

        var result = slot.Book();

        result.IsSuccess.Should().BeTrue();
        slot.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Book_WhenUnavailable_ShouldReturnFailure()
    {
        var slot = new ScheduleSlot(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(9), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)));
        slot.Book();

        var result = slot.Book();

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ErrorCodes.ScheduleSlot.NotAvailable.Code);
    }

    [Fact]
    public void Block_WhenAvailable_ShouldMarkUnavailable()
    {
        var slot = new ScheduleSlot(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(9), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)));

        var result = slot.Block();

        result.IsSuccess.Should().BeTrue();
        slot.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Unblock_WhenBlockedWithoutBooking_ShouldMarkAvailable()
    {
        var slot = new ScheduleSlot(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(9), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)));
        slot.Block();

        var result = slot.Unblock();

        result.IsSuccess.Should().BeTrue();
        slot.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void CancelBooking_ShouldReleaseSlot()
    {
        var slot = new ScheduleSlot(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(9), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(30)));
        slot.Book();

        var result = slot.CancelBooking();

        result.IsSuccess.Should().BeTrue();
        slot.IsAvailable.Should().BeTrue();
    }
}
