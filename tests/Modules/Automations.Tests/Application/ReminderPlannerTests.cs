using Automations.Application.Reminders;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Automations.Domain.ValueObjects;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Automations.Tests.Application;

public class ReminderPlannerTests
{
    [Fact]
    public async Task EnqueueAsync_SkipsWhenTutorOptedOutOfWhatsApp()
    {
        var tutorId = Guid.NewGuid();
        var tutor = CreateTutor(tutorId);
        var jobRepo = Substitute.For<IMessageJobRepository>();
        jobRepo.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((MessageJob?)null);

        var prefRepo = Substitute.For<ITutorMessagingPreferenceRepository>();
        prefRepo.GetByTutorIdAsync(tutorId, Arg.Any<CancellationToken>())
            .Returns(TutorMessagingPreference.Create(tutorId, whatsAppEnabled: false, emailEnabled: true).Value);

        var tutorRepo = Substitute.For<ITutorRepository>();
        tutorRepo.GetByIdAsync(tutorId, Arg.Any<CancellationToken>()).Returns(tutor);

        var tenant = Substitute.For<ITenantContext>();
        tenant.TenantId.Returns(Guid.NewGuid());

        var planner = new ReminderPlanner(jobRepo, prefRepo, tutorRepo, tenant);
        var candidate = new ReminderCandidate(
            ReminderKind.Vaccine,
            tutorId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "reminder.vaccine",
            "reminder:vaccine:test:d-7",
            new Dictionary<string, string> { ["PetName"] = "Thor", ["VaccineName"] = "V8", ["WhenLocal"] = "01/10/2026" });

        var count = await planner.EnqueueAsync([candidate], CancellationToken.None);

        count.Should().Be(1);
        jobRepo.Received(1).Add(Arg.Is<MessageJob>(j => j.Channel == MessageChannel.Email));
    }

    [Fact]
    public async Task EnqueueAsync_DoesNotEnqueueSms()
    {
        var planner = new ReminderPlanner(
            Substitute.For<IMessageJobRepository>(),
            Substitute.For<ITutorMessagingPreferenceRepository>(),
            Substitute.For<ITutorRepository>(),
            Substitute.For<ITenantContext>());

        var count = await planner.EnqueueAsync([], CancellationToken.None);
        count.Should().Be(0);
    }

    private static Tutor CreateTutor(Guid id)
    {
        var email = Email.Create("tutor@test.com").Value;
        var cpf = Cpf.Create("52998224725").Value;
        var phone = Phone.Create("11987654321").Value;
        return Tutor.Create("Ana", email, cpf, phone, id).Value;
    }
}
