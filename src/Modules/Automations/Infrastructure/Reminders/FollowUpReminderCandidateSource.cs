using Automations.Application.Abstractions;
using Automations.Application.Reminders;
using Automations.Domain.Repositories;
using Automations.Infrastructure.Configuration;
using Core.Domain;
using Microsoft.Extensions.Options;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;

namespace Automations.Infrastructure.Reminders;

/// <summary>
/// Reminds tutors one day before a scheduled clinical follow-up date on a finalized record.
/// </summary>
public sealed class FollowUpReminderCandidateSource : IReminderCandidateSource
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAutomationsSettingsRepository _settingsRepository;
    private readonly AutomationsOptions _options;

    public FollowUpReminderCandidateSource(
        IMedicalRecordRepository medicalRecordRepository,
        IAppointmentRepository appointmentRepository,
        IPetRepository petRepository,
        IAutomationsSettingsRepository settingsRepository,
        IOptions<AutomationsOptions> options)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _appointmentRepository = appointmentRepository;
        _petRepository = petRepository;
        _settingsRepository = settingsRepository;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReminderCandidate>> CollectAsync(DateOnly localToday, CancellationToken cancellationToken)
    {
        var tz = await ResolveTimeZoneAsync(cancellationToken);
        var followUpDay = localToday.AddDays(1);
        var records = await _medicalRecordRepository.ListFinalizedWithFollowUpOnAsync(followUpDay, cancellationToken);
        var dayStart = ReminderTimeHelper.DayStartUtc(followUpDay, tz);
        var appointments = (await _appointmentRepository.GetByDayAsync(null, dayStart, cancellationToken)).ToList();
        var results = new List<ReminderCandidate>();

        foreach (var record in records)
        {
            var hasAppointment = appointments.Any(a =>
                a.PetId == record.PetId
                && a.Status is AppointmentStatus.Scheduled or AppointmentStatus.Confirmed);

            if (hasAppointment)
            {
                continue;
            }

            var pet = await _petRepository.GetByIdAsync(record.PetId, cancellationToken);
            if (pet is null || pet.IsDeleted)
            {
                continue;
            }

            results.Add(new ReminderCandidate(
                ReminderKind.FollowUp,
                record.TutorId,
                record.PetId,
                record.Id,
                "reminder.followup",
                $"reminder:followup:{record.Id:N}:d-1",
                new Dictionary<string, string>
                {
                    ["PetName"] = pet.Name,
                    ["WhenLocal"] = followUpDay.ToString("dd/MM/yyyy")
                }));
        }

        return results;
    }

    private async Task<TimeZoneInfo> ResolveTimeZoneAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetSingletonAsync(cancellationToken);
        if (settings is not null)
        {
            return settings.ResolveTimeZone();
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}
