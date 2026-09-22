using Automations.Application.Abstractions;
using Automations.Application.Reminders;
using Automations.Domain.Repositories;
using Automations.Infrastructure.Configuration;
using Core.Domain;
using Microsoft.Extensions.Options;
using Veterinary.Domain.Repositories;

namespace Automations.Infrastructure.Reminders;

/// <summary>
/// Enqueues vaccine booster reminders exactly seven local days before due date.
/// </summary>
public sealed class VaccineReminderCandidateSource : IReminderCandidateSource
{
    private readonly IVaccineDoseRepository _doseRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAutomationsSettingsRepository _settingsRepository;
    private readonly AutomationsOptions _options;

    public VaccineReminderCandidateSource(
        IVaccineDoseRepository doseRepository,
        IPetRepository petRepository,
        IAutomationsSettingsRepository settingsRepository,
        IOptions<AutomationsOptions> options)
    {
        _doseRepository = doseRepository;
        _petRepository = petRepository;
        _settingsRepository = settingsRepository;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReminderCandidate>> CollectAsync(DateOnly localToday, CancellationToken cancellationToken)
    {
        var tz = await ResolveTimeZoneAsync(cancellationToken);
        var dueTarget = localToday.AddDays(7);
        var utcNow = DateTimeOffset.UtcNow;
        var until = utcNow.AddDays(10);
        var doses = await _doseRepository.GetDueAsync(utcNow, until, take: 500, cancellationToken);
        var results = new List<ReminderCandidate>();

        foreach (var dose in doses)
        {
            if (dose.NextDueDate is null)
            {
                continue;
            }

            if (ReminderTimeHelper.ToLocalDate(dose.NextDueDate.Value, tz) != dueTarget)
            {
                continue;
            }

            var pet = await _petRepository.GetByIdAsync(dose.PetId, cancellationToken);
            if (pet is null || pet.IsDeleted)
            {
                continue;
            }

            var whenLocal = dose.NextDueDate.Value.ToString("dd/MM/yyyy");
            results.Add(new ReminderCandidate(
                ReminderKind.Vaccine,
                pet.TutorId,
                pet.Id,
                dose.Id,
                "reminder.vaccine",
                $"reminder:vaccine:{dose.Id:N}:d-7:{dueTarget:yyyy-MM-dd}",
                new Dictionary<string, string>
                {
                    ["PetName"] = pet.Name,
                    ["VaccineName"] = dose.Name,
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
