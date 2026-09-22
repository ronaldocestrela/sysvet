using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.TutorPortal;

/// <summary>
/// HTTP client for tutor portal authentication and profile APIs.
/// </summary>
public sealed class TutorPortalApiService
{
    private readonly ApiClient _apiClient;

    public TutorPortalApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>Self-registers a tutor when CRM email and CPF match.</summary>
    public Task<Result<TutorAuthTokensDto>> RegisterAsync(TutorRegisterRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<TutorRegisterRequest, TutorAuthTokensDto>("/api/v1/tutor-portal/register", request, cancellationToken: cancellationToken);

    /// <summary>Authenticates a tutor portal user.</summary>
    public Task<Result<TutorAuthTokensDto>> LoginAsync(TutorLoginRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<TutorLoginRequest, TutorAuthTokensDto>("/api/v1/tutor-portal/login", request, cancellationToken: cancellationToken);

    /// <summary>Loads the authenticated tutor profile.</summary>
    public Task<Result<TutorPortalMeDto>> GetMeAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<TutorPortalMeDto>("/api/v1/tutor-portal/me", cancellationToken);

    /// <summary>Loads vaccination card for an owned pet.</summary>
    public Task<Result<TutorVaccinationCardDto>> GetVaccinationCardAsync(Guid petId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<TutorVaccinationCardDto>($"/api/v1/tutor-portal/pets/{petId}/vaccination-card", cancellationToken);

    /// <summary>Lists exams for an owned pet.</summary>
    public Task<Result<List<TutorPetExamDto>>> GetExamsAsync(Guid petId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<List<TutorPetExamDto>>($"/api/v1/tutor-portal/pets/{petId}/exams", cancellationToken);

    /// <summary>Lists visit timeline for an owned pet.</summary>
    public Task<Result<List<TutorPetTimelineItemDto>>> GetTimelineAsync(Guid petId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<List<TutorPetTimelineItemDto>>($"/api/v1/tutor-portal/pets/{petId}/timeline", cancellationToken);

    /// <summary>Returns server VAPID public key when push is enabled.</summary>
    public Task<Result<string?>> GetVapidPublicKeyAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<string?>("/api/v1/tutor-portal/push/vapid-public-key", cancellationToken);

    /// <summary>Registers browser push subscription.</summary>
    public Task<Result> SubscribePushAsync(TutorPushSubscribeRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync("/api/v1/tutor-portal/push/subscribe", request, cancellationToken: cancellationToken);

    /// <summary>Lists bookable services for tutor scheduling.</summary>
    public Task<Result<List<TutorBookableServiceDto>>> GetBookableServicesAsync(Guid petId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<List<TutorBookableServiceDto>>($"/api/v1/tutor-portal/pets/{petId}/booking/services", cancellationToken);

    /// <summary>Lists professionals available on a day.</summary>
    public Task<Result<List<TutorBookingProfessionalDto>>> GetBookingProfessionalsAsync(
        Guid petId,
        TutorBookingKindDto kind,
        Guid serviceId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<List<TutorBookingProfessionalDto>>(
            $"/api/v1/tutor-portal/pets/{petId}/booking/professionals?kind={kind}&serviceId={serviceId}&date={Uri.EscapeDataString(date.ToString("O"))}",
            cancellationToken);

    /// <summary>Lists bookable slot starts.</summary>
    public Task<Result<List<TutorAvailableSlotDto>>> GetAvailableSlotsAsync(
        Guid petId,
        TutorBookingKindDto kind,
        Guid serviceId,
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<List<TutorAvailableSlotDto>>(
            $"/api/v1/tutor-portal/pets/{petId}/booking/slots?kind={kind}&serviceId={serviceId}&professionalId={professionalId}&date={Uri.EscapeDataString(date.ToString("O"))}",
            cancellationToken);

    /// <summary>Lists upcoming appointments for a pet.</summary>
    public Task<Result<List<TutorPetAppointmentDto>>> GetPetAppointmentsAsync(Guid petId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<List<TutorPetAppointmentDto>>($"/api/v1/tutor-portal/pets/{petId}/booking/appointments", cancellationToken);

    /// <summary>Books an appointment for a pet.</summary>
    public Task<Result<Guid>> BookAppointmentAsync(Guid petId, TutorBookAppointmentRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<TutorBookAppointmentRequest, Guid>($"/api/v1/tutor-portal/pets/{petId}/booking/appointments", request, cancellationToken: cancellationToken);

    /// <summary>Cancels a tutor-owned appointment.</summary>
    public Task<Result> CancelAppointmentAsync(Guid petId, Guid appointmentId, TutorBookingKindDto kind, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync($"/api/v1/tutor-portal/pets/{petId}/booking/appointments/{appointmentId}/cancel", new { kind }, cancellationToken: cancellationToken);
}

/// <summary>Registration payload for tutor portal.</summary>
public sealed class TutorRegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Login payload for tutor portal.</summary>
public sealed class TutorLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>JWT pair returned by tutor portal auth endpoints.</summary>
public sealed class TutorAuthTokensDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}

/// <summary>Profile returned by tutor portal <c>/me</c>.</summary>
public sealed class TutorPortalMeDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid TutorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TutorPortalPetDto> Pets { get; set; } = [];
}

/// <summary>Pet summary on tutor portal home.</summary>
public sealed class TutorPortalPetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>Tutor portal vaccination card.</summary>
public sealed class TutorVaccinationCardDto
{
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public List<TutorVaccineDoseDto> AppliedDoses { get; set; } = [];
    public List<TutorSuggestedVaccineDoseDto> SuggestedDoses { get; set; } = [];
}

/// <summary>Applied dose on tutor card.</summary>
public sealed class TutorVaccineDoseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
    public DateTimeOffset? NextDueDate { get; set; }
}

/// <summary>Suggested dose on tutor card.</summary>
public sealed class TutorSuggestedVaccineDoseDto
{
    public string ProtocolName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Sequence { get; set; }
}

/// <summary>Exam row for tutor portal.</summary>
public sealed class TutorPetExamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string ResultSummary { get; set; } = string.Empty;
}

/// <summary>Timeline row for tutor portal.</summary>
public sealed class TutorPetTimelineItemDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>Clinical vs grooming booking kind (matches API enum).</summary>
public enum TutorBookingKindDto
{
    Clinical = 0,
    Grooming = 1
}

/// <summary>Virtual clinical consultation service id.</summary>
public static class TutorBookingClientConstants
{
    public static readonly Guid ClinicalConsultationServiceId = new("00000000-0000-4000-8000-000000000001");
}

/// <summary>Bookable service row.</summary>
public sealed class TutorBookableServiceDto
{
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
}

/// <summary>Professional available for booking.</summary>
public sealed class TutorBookingProfessionalDto
{
    public Guid ProfessionalId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>Available slot start.</summary>
public sealed class TutorAvailableSlotDto
{
    public DateTimeOffset Start { get; set; }
    public int DurationInMinutes { get; set; }
}

/// <summary>Pet appointment summary.</summary>
public sealed class TutorPetAppointmentDto
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool CanCancel { get; set; }
}

/// <summary>Book appointment request body.</summary>
public sealed class TutorBookAppointmentRequest
{
    public TutorBookingKindDto Kind { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ProfessionalId { get; set; }
    public DateTimeOffset Date { get; set; }
    public Guid Id { get; set; }
}

/// <summary>Push subscription registration payload.</summary>
public sealed class TutorPushSubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
}
