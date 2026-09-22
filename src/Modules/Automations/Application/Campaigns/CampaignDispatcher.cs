using System.Text.Json;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Domain;
using Core.Domain.Entities;

namespace Automations.Application.Campaigns;

/// <summary>
/// Enqueues marketing and NPS jobs respecting channel and marketing opt-out.
/// </summary>
public sealed class CampaignDispatcher
{
    private readonly IMessageJobRepository _jobRepository;
    private readonly ITutorMessagingPreferenceRepository _preferenceRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly ITenantContext _tenantContext;

    public CampaignDispatcher(
        IMessageJobRepository jobRepository,
        ITutorMessagingPreferenceRepository preferenceRepository,
        ITutorRepository tutorRepository,
        ITenantContext tenantContext)
    {
        _jobRepository = jobRepository;
        _preferenceRepository = preferenceRepository;
        _tutorRepository = tutorRepository;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Enqueues WhatsApp and/or e-mail jobs for each target when marketing consent allows.
    /// </summary>
    public async Task<int> EnqueueAsync(IReadOnlyList<CampaignDispatchTarget> targets, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId == Guid.Empty)
        {
            return 0;
        }

        var enqueued = 0;
        foreach (var target in targets)
        {
            var tutor = await _tutorRepository.GetByIdAsync(target.TutorId, cancellationToken);
            if (tutor is null || tutor.IsDeleted)
            {
                continue;
            }

            var pref = await _preferenceRepository.GetByTutorIdAsync(target.TutorId, cancellationToken)
                       ?? TutorMessagingPreference.DefaultFor(target.TutorId);

            if (!pref.MarketingEnabled)
            {
                continue;
            }

            var basePayload = new Dictionary<string, string>(target.Tokens)
            {
                ["TutorId"] = tutor.Id.ToString("N"),
                ["TutorName"] = tutor.Name,
                ["ToPhone"] = tutor.Phone.Number,
                ["ToEmail"] = tutor.Email.Address
            };

            if (pref.WhatsAppEnabled && !string.IsNullOrWhiteSpace(tutor.Phone.Number))
            {
                enqueued += await TryEnqueueChannelAsync(MessageChannel.WhatsApp, target, basePayload, cancellationToken);
            }

            if (pref.EmailEnabled && !string.IsNullOrWhiteSpace(tutor.Email.Address))
            {
                enqueued += await TryEnqueueChannelAsync(MessageChannel.Email, target, basePayload, cancellationToken);
            }
        }

        return enqueued;
    }

    private async Task<int> TryEnqueueChannelAsync(
        MessageChannel channel,
        CampaignDispatchTarget target,
        Dictionary<string, string> payload,
        CancellationToken cancellationToken)
    {
        if (channel == MessageChannel.Sms)
        {
            return 0;
        }

        var idempotencyKey = $"{target.IdempotencyKeyBase}:{channel.ToString().ToLowerInvariant()}";
        var existing = await _jobRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return 0;
        }

        var payloadJson = JsonSerializer.Serialize(payload);
        var created = MessageJob.Enqueue(
            _tenantContext.TenantId,
            channel,
            target.TemplateCode,
            payloadJson,
            idempotencyKey,
            sourceType: target.SourceType,
            sourceId: target.SourceId);

        if (created.IsFailure)
        {
            return 0;
        }

        _jobRepository.Add(created.Value);
        return 1;
    }
}
