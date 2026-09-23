using Automations.Domain.Repositories;
using Core.Application.Privacy;
using Core.Domain;

namespace Automations.Infrastructure.Privacy;

/// <summary>Automations module personal-data export and erasure for messaging preferences.</summary>
public sealed class AutomationsPersonalDataContributor : IPersonalDataExportContributor, IPersonalDataErasureContributor
{
    private readonly ITutorMessagingPreferenceRepository _preferenceRepository;

    /// <summary>Initializes repository dependency.</summary>
    public AutomationsPersonalDataContributor(ITutorMessagingPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    /// <inheritdoc />
    public string ModuleKey => "Automations";

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, object?>> GetSlicesAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        var pref = await _preferenceRepository.GetByTutorIdAsync(tutorId, cancellationToken);
        if (pref is null)
        {
            return new Dictionary<string, object?> { ["messagingPreference"] = null };
        }

        return new Dictionary<string, object?>
        {
            ["messagingPreference"] = new Dictionary<string, object?>
            {
                ["whatsAppEnabled"] = pref.WhatsAppEnabled,
                ["emailEnabled"] = pref.EmailEnabled,
                ["marketingEnabled"] = pref.MarketingEnabled
            }
        };
    }

    /// <inheritdoc />
    public async Task<Result> EraseForTutorAsync(PersonalDataErasureContext context, CancellationToken cancellationToken)
    {
        var pref = await _preferenceRepository.GetByTutorIdAsync(context.TutorId, cancellationToken);
        if (pref is null)
        {
            return Result.Success();
        }

        pref.Update(whatsAppEnabled: false, emailEnabled: false, marketingEnabled: false);
        _preferenceRepository.Update(pref);
        return Result.Success();
    }
}
