using API.Hubs;
using Core.Application.IntegrationEvents;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace API.Notifications;

/// <summary>
/// Pushes grooming status changes to connected clinic clients via SignalR.
/// </summary>
public sealed class GroomingRealtimeBroadcastHandler : INotificationHandler<GroomingStatusChangedEvent>
{
    private readonly IHubContext<GroomingStatusHub> _hubContext;
    private readonly ILogger<GroomingRealtimeBroadcastHandler> _logger;

    public GroomingRealtimeBroadcastHandler(
        IHubContext<GroomingStatusHub> hubContext,
        ILogger<GroomingRealtimeBroadcastHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(GroomingStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new GroomingStatusChangedMessage(
                notification.GroomingAppointmentId,
                notification.PetId,
                notification.TutorId,
                notification.Kind.ToString(),
                notification.Status,
                notification.OccurredOn);

            await _hubContext.Clients
                .Group(GroomingStatusHub.TenantGroup(notification.TenantId))
                .SendAsync("GroomingStatusChanged", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "SignalR broadcast failed for grooming appointment {AppointmentId}",
                notification.GroomingAppointmentId);
        }
    }
}

/// <summary>
/// Client payload for grooming status hub events.
/// </summary>
public sealed record GroomingStatusChangedMessage(
    Guid GroomingAppointmentId,
    Guid PetId,
    Guid TutorId,
    string Kind,
    string Status,
    DateTimeOffset OccurredOn);
