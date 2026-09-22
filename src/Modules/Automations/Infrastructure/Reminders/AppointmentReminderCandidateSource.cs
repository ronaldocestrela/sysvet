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
/// Reminds tutors one local day before a clinical appointment.
/// </summary>
public sealed class AppointmentReminderCandidateSource : IReminderCandidateSource
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAutomationsSettingsRepository _settingsRepository;
    private readonly AutomationsOptions _options;

    public AppointmentReminderCandidateSource(
        IAppointmentRepository appointmentRepository,
        IPetRepository petRepository,
        IAutomationsSettingsRepository settingsRepository,
        IOptions<AutomationsOptions> options)
    {
        _appointmentRepository = appointmentRepository;
        _petRepository = petRepository;
        _settingsRepository = settingsRepository;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReminderCandidate>> CollectAsync(DateOnly localToday, CancellationToken cancellationToken)
    {
        var tz = await ResolveTimeZoneAsync(cancellationToken);
        var tomorrow = localToday.AddDays(1);
        var dayStart = ReminderTimeHelper.DayStartUtc(tomorrow, tz);
        var appointments = await _appointmentRepository.GetByDayAsync(null, dayStart, cancellationToken);
        var results = new List<ReminderCandidate>();

        foreach (var appointment in appointments)
        {
            if (appointment.Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
            {
                continue;
            }

            var pet = await _petRepository.GetByIdAsync(appointment.PetId, cancellationToken);
            if (pet is null || pet.IsDeleted)
            {
                continue;
            }

            var whenLocal = TimeZoneInfo.ConvertTime(appointment.Date, tz).ToString("dd/MM/yyyy HH:mm");
            results.Add(new ReminderCandidate(
                ReminderKind.Appointment,
                appointment.TutorId,
                appointment.PetId,
                appointment.Id,
                "reminder.appointment",
                $"reminder:appointment:{appointment.Id:N}:d-1",
                new Dictionary<string, string>
                {
                    ["PetName"] = pet.Name,
                    ["WhenLocal"] = whenLocal
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
