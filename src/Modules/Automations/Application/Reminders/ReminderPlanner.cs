using System.Text.Json;
using Automations.Application.Abstractions;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Automations.Application.Reminders;

/// <summary>
/// Turns reminder candidates into durable message jobs respecting opt-out and channel availability.
/// </summary>
public sealed class ReminderPlanner
{
    private readonly IMessageJobRepository _jobRepository;
    private readonly ITutorMessagingPreferenceRepository _preferenceRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly ITenantContext _tenantContext;

    public ReminderPlanner(
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
    /// Enqueues WhatsApp and/or e-mail jobs for each candidate when allowed.
    /// </summary>
    public async Task<int> EnqueueAsync(IReadOnlyList<ReminderCandidate> candidates, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId == Guid.Empty)
        {
            return 0;
        }

        var enqueued = 0;
        foreach (var candidate in candidates)
        {
            var tutor = await _tutorRepository.GetByIdAsync(candidate.TutorId, cancellationToken);
            if (tutor is null || tutor.IsDeleted)
            {
                continue;
            }

            var pref = await _preferenceRepository.GetByTutorIdAsync(candidate.TutorId, cancellationToken)
                       ?? TutorMessagingPreference.DefaultFor(candidate.TutorId);

            var basePayload = new Dictionary<string, string>(candidate.Tokens)
            {
                ["TutorId"] = tutor.Id.ToString("N"),
                ["TutorName"] = tutor.Name,
                ["ToPhone"] = tutor.Phone.Number,
                ["ToEmail"] = tutor.Email.Address
            };

            if (pref.WhatsAppEnabled && !string.IsNullOrWhiteSpace(tutor.Phone.Number))
            {
                enqueued += await TryEnqueueChannelAsync(
                    MessageChannel.WhatsApp,
                    candidate,
                    basePayload,
                    cancellationToken);
            }

            if (pref.EmailEnabled && !string.IsNullOrWhiteSpace(tutor.Email.Address))
            {
                enqueued += await TryEnqueueChannelAsync(
                    MessageChannel.Email,
                    candidate,
                    basePayload,
                    cancellationToken);
            }
        }

        return enqueued;
    }

    private async Task<int> TryEnqueueChannelAsync(
        MessageChannel channel,
        ReminderCandidate candidate,
        Dictionary<string, string> payload,
        CancellationToken cancellationToken)
    {
        if (channel == MessageChannel.Sms)
        {
            return 0;
        }

        var idempotencyKey = $"{candidate.IdempotencyKeyBase}:{channel.ToString().ToLowerInvariant()}";
        var existing = await _jobRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return 0;
        }

        var payloadJson = JsonSerializer.Serialize(payload);
        var created = MessageJob.Enqueue(
            _tenantContext.TenantId,
            channel,
            candidate.TemplateCode,
            payloadJson,
            idempotencyKey,
            sourceType: candidate.Kind.ToString(),
            sourceId: candidate.SourceId);

        if (created.IsFailure)
        {
            return 0;
        }

        _jobRepository.Add(created.Value);
        return 1;
    }
}
