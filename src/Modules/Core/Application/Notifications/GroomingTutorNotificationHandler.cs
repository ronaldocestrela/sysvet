using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Notifications;

/// <summary>
/// Sends tutor grooming notifications when an external channel is enabled.
/// </summary>
public sealed class GroomingTutorNotificationHandler : INotificationHandler<GroomingStatusChangedEvent>
{
    private readonly ITutorNotificationChannel _channel;
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;
    private readonly ILogger<GroomingTutorNotificationHandler> _logger;

    public GroomingTutorNotificationHandler(
        ITutorNotificationChannel channel,
        ITutorRepository tutorRepository,
        IPetRepository petRepository,
        ILogger<GroomingTutorNotificationHandler> logger)
    {
        _channel = channel;
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
        _logger = logger;
    }

    public async Task Handle(GroomingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        if (!_channel.IsEnabled)
        {
            return;
        }

        if (notification.Kind is not (GroomingNotificationKind.Started or GroomingNotificationKind.ReadyForPickup))
        {
            return;
        }

        try
        {
            var tutor = await _tutorRepository.GetByIdAsync(notification.TutorId, cancellationToken);
            var pet = await _petRepository.GetByIdAsync(notification.PetId, cancellationToken);
            if (tutor is null || pet is null)
            {
                return;
            }

            await _channel.NotifyGroomingStatusAsync(
                new TutorGroomingNotification(
                    notification.TenantId,
                    notification.TutorId,
                    notification.PetId,
                    notification.GroomingAppointmentId,
                    notification.Kind,
                    tutor.Phone.Number,
                    tutor.Name,
                    pet.Name),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Tutor notification failed for grooming appointment {AppointmentId}",
                notification.GroomingAppointmentId);
        }
    }
}
