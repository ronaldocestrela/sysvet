using System.Text.Json;
using Automations.Application.Abstractions;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Automations.Domain.Services;
using Core.Domain;

namespace Automations.Application.Jobs;

/// <summary>
/// Processes a single claimed message job: render template, send, update status and attempt log.
/// </summary>
public sealed class MessageJobProcessor
{
    private readonly IMessageJobRepository _jobRepository;
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly IOutboundMessageSender _sender;
    private readonly IAutomationsDeliveryPolicy _deliveryPolicy;
    private readonly ITutorMessagingPreferenceRepository _preferenceRepository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public MessageJobProcessor(
        IMessageJobRepository jobRepository,
        IMessageTemplateRepository templateRepository,
        IOutboundMessageSender sender,
        IAutomationsDeliveryPolicy deliveryPolicy,
        ITutorMessagingPreferenceRepository preferenceRepository,
        IAutomationsUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _templateRepository = templateRepository;
        _sender = sender;
        _deliveryPolicy = deliveryPolicy;
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Claims and processes due jobs up to <paramref name="batchSize"/>.
    /// </summary>
    public async Task<int> ProcessDueAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var due = await _jobRepository.ListDueAsync(batchSize, now, cancellationToken);
        var processed = 0;
        foreach (var job in due)
        {
            await ProcessOneAsync(job.Id, now, cancellationToken);
            processed++;
        }

        return processed;
    }

    /// <summary>
    /// Loads and processes one job by id (used by tests and worker).
    /// </summary>
    public async Task ProcessOneAsync(Guid jobId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            return;
        }

        job.Claim(now);
        var startedAt = now;

        if (job.Channel == MessageChannel.Sms)
        {
            var finishedAt = DateTimeOffset.UtcNow;
            job.MarkDeadLetter(Automations.Domain.ErrorCodes.Channel.SmsNotSupported.Message, startedAt, finishedAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var policy = await _deliveryPolicy.EvaluateAsync(now, cancellationToken);
        if (!policy.CanDeliver)
        {
            job.DeferUntil(policy.DeferUntil);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var tokens = ParsePayloadTokens(job.PayloadJson);
        if (tokens is null)
        {
            var finishedAt = DateTimeOffset.UtcNow;
            job.MarkDeadLetter("Invalid payload JSON.", startedAt, finishedAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (!await IsChannelAllowedForTutorAsync(job.Channel, job.TemplateCode, tokens, cancellationToken))
        {
            var finishedAt = DateTimeOffset.UtcNow;
            job.MarkDeadLetter("Tutor opted out of this channel or marketing.", startedAt, finishedAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var template = await _templateRepository.GetByCodeAndChannelAsync(job.TemplateCode, job.Channel, cancellationToken);
        if (template is null || !template.IsActive)
        {
            var finishedAt = DateTimeOffset.UtcNow;
            job.ScheduleRetry("Template not found or inactive.", startedAt, finishedAt, finishedAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var rendered = MessageTemplateRenderer.Render(template.Body, tokens);
        if (rendered.IsFailure)
        {
            var finishedAt = DateTimeOffset.UtcNow;
            job.MarkDeadLetter(rendered.Error.Message, startedAt, finishedAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        tokens.TryGetValue("ToPhone", out var toPhone);
        tokens.TryGetValue("ToEmail", out var toEmail);

        var sendResult = await _sender.SendAsync(
            job.Channel,
            template.Subject,
            rendered.Value,
            job.PayloadJson,
            toPhone,
            toEmail,
            cancellationToken);

        var endAt = DateTimeOffset.UtcNow;
        if (sendResult.IsSuccess)
        {
            job.MarkSucceeded("Delivered.", startedAt, endAt);
        }
        else
        {
            job.ScheduleRetry(sendResult.Error.Message, startedAt, endAt, endAt);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> IsChannelAllowedForTutorAsync(
        MessageChannel channel,
        string templateCode,
        IReadOnlyDictionary<string, string> tokens,
        CancellationToken cancellationToken)
    {
        if (!tokens.TryGetValue("TutorId", out var tutorIdRaw) || !Guid.TryParse(tutorIdRaw, out var tutorId))
        {
            return true;
        }

        var pref = await _preferenceRepository.GetByTutorIdAsync(tutorId, cancellationToken)
                   ?? TutorMessagingPreference.DefaultFor(tutorId);

        if (MarketingMessageClassifier.RequiresMarketingConsent(templateCode) && !pref.MarketingEnabled)
        {
            return false;
        }

        return channel switch
        {
            MessageChannel.WhatsApp => pref.WhatsAppEnabled,
            MessageChannel.Email => pref.EmailEnabled,
            _ => false
        };
    }

    private static IReadOnlyDictionary<string, string>? ParsePayloadTokens(string payloadJson)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(payloadJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
