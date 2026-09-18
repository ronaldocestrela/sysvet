using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>
/// Client port for clinical agenda — offline-first with sync outbox.
/// </summary>
public interface IAppointmentStore
{
    Task<Result<List<AppointmentListItemDto>>> GetDailyAsync(Guid? veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task<Result<Guid>> ScheduleAsync(ScheduleAppointmentRequest request, CancellationToken cancellationToken = default);

    Task<Result> ConfirmAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> StartAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}

/// <summary>Appointment row for SharedUI day view.</summary>
public sealed class AppointmentListItemDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public DateTimeOffset Date { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PetName { get; init; } = string.Empty;
}

/// <summary>Schedule request from UI.</summary>
public sealed class ScheduleAppointmentRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public DateTimeOffset Date { get; init; }
    public int DurationInMinutes { get; init; } = 30;
    public string Reason { get; init; } = string.Empty;
}
