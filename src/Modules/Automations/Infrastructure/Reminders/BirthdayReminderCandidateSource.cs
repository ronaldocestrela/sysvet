using Automations.Application.Abstractions;
using Automations.Application.Reminders;
using Core.Domain;

namespace Automations.Infrastructure.Reminders;

/// <summary>
/// Sends pet birthday greetings on the local calendar day.
/// </summary>
public sealed class BirthdayReminderCandidateSource : IReminderCandidateSource
{
    private readonly IPetRepository _petRepository;

    public BirthdayReminderCandidateSource(IPetRepository petRepository) => _petRepository = petRepository;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReminderCandidate>> CollectAsync(DateOnly localToday, CancellationToken cancellationToken)
    {
        var pets = await _petRepository.GetAllAsync(cancellationToken);
        var results = new List<ReminderCandidate>();

        foreach (var pet in pets.Where(p => !p.IsDeleted && p.BirthDate is not null))
        {
            if (!ReminderTimeHelper.IsBirthdayToday(pet.BirthDate, localToday))
            {
                continue;
            }

            results.Add(new ReminderCandidate(
                ReminderKind.Birthday,
                pet.TutorId,
                pet.Id,
                pet.Id,
                "reminder.birthday",
                $"reminder:birthday:{pet.Id:N}:{localToday.Year}",
                new Dictionary<string, string>
                {
                    ["PetName"] = pet.Name,
                    ["WhenLocal"] = localToday.ToString("dd/MM/yyyy")
                }));
        }

        return results;
    }
}
