using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.Events;
using Core.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Domain;

public class AggregateRootTests
{
    [Fact]
    public void CreateTutor_ShouldRaiseTutorRegisteredDomainEvent()
    {
        var email = Email.Create("maria@example.com").Value;
        var cpf = Cpf.Create("12345678909").Value;
        var phone = Phone.Create("11999998888").Value;

        var result = Tutor.Create("Maria Silva", email, cpf, phone);

        result.IsSuccess.Should().BeTrue();
        var tutor = result.Value;
        tutor.DomainEvents.Should().ContainSingle(e => e is TutorRegisteredDomainEvent);
        var domainEvent = (TutorRegisteredDomainEvent)tutor.DomainEvents.Single();
        domainEvent.TutorId.Should().Be(tutor.Id);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var tutor = Tutor.Create(
            "Maria Silva",
            Email.Create("maria@example.com").Value,
            Cpf.Create("12345678909").Value,
            Phone.Create("11999998888").Value).Value;

        tutor.DomainEvents.Should().NotBeEmpty();
        tutor.ClearDomainEvents();
        tutor.DomainEvents.Should().BeEmpty();
    }
}
