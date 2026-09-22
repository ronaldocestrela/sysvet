using System.Text.Json;
using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Core.Application.IntegrationEvents;
using Core.Application.Notifications;
using Core.Domain;

namespace Automations.Infrastructure.Notifications;

/// <summary>
/// Enqueues grooming tutor notifications as durable Automations jobs (ADR-031 / ADR-038).
/// </summary>
public sealed class EnqueueingTutorNotificationChannel : ITutorNotificationChannel
{
    private readonly IMessageJobRepository _jobRepository;
    private readonly ITutorMessagingPreferenceRepository _preferenceRepository;
    private readonly IAutomationsUnitOfWork _unitOfWork;

    public EnqueueingTutorNotificationChannel(
        IMessageJobRepository jobRepository,
        ITutorMessagingPreferenceRepository preferenceRepository,
        IAutomationsUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public async Task NotifyGroomingStatusAsync(TutorGroomingNotification notification, CancellationToken cancellationToken = default)
    {
        var templateCode = notification.Kind switch
        {
            GroomingNotificationKind.Started => "grooming.started",
            GroomingNotificationKind.ReadyForPickup => "grooming.ready",
            _ => null
        };

        if (templateCode is null)
        {
            return;
        }

        var pref = await _preferenceRepository.GetByTutorIdAsync(notification.TutorId, cancellationToken)
                   ?? TutorMessagingPreference.DefaultFor(notification.TutorId);
        if (!pref.WhatsAppEnabled)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["TutorId"] = notification.TutorId.ToString("N"),
            ["TutorName"] = notification.TutorName,
            ["PetName"] = notification.PetName,
            ["TutorPhone"] = notification.TutorPhone,
            ["ToPhone"] = notification.TutorPhone
        });

        var idempotencyKey = $"grooming:{notification.GroomingAppointmentId:N}:{(int)notification.Kind}";
        var existing = await _jobRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var created = MessageJob.Enqueue(
            notification.TenantId,
            MessageChannel.WhatsApp,
            templateCode,
            payload,
            idempotencyKey,
            sourceType: "GroomingAppointment",
            sourceId: notification.GroomingAppointmentId);

        if (created.IsFailure)
        {
            return;
        }

        _jobRepository.Add(created.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
