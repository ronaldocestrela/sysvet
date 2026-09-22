using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Automations;

/// <summary>
/// Online API client for Automations templates and job logs (Fase 8.1).
/// </summary>
public sealed class AutomationsApiService
{
    private readonly ApiClient _apiClient;
    private readonly Sync.ISyncConnectivity _connectivity;

    public AutomationsApiService(ApiClient apiClient, Sync.ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    /// <summary>Lists message templates.</summary>
    public Task<Result<IReadOnlyList<MessageTemplateClientDto>>> ListTemplatesAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<MessageTemplateClientDto>>(OfflineError()));
        }

        return _apiClient.GetAsync<IReadOnlyList<MessageTemplateClientDto>>("/api/v1/automations/templates", cancellationToken);
    }

    /// <summary>Gets tenant Automations settings.</summary>
    public Task<Result<AutomationsSettingsClientDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<AutomationsSettingsClientDto>(OfflineError()));
        }

        return _apiClient.GetAsync<AutomationsSettingsClientDto>("/api/v1/automations/settings", cancellationToken);
    }

    /// <summary>Updates tenant business hours settings.</summary>
    public Task<Result> UpdateSettingsAsync(AutomationsSettingsClientDto dto, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure(OfflineError()));
        }

        return _apiClient.PutAsync("/api/v1/automations/settings", dto, idempotencyKey: null, cancellationToken);
    }

    /// <summary>Gets tutor messaging preferences.</summary>
    public Task<Result<TutorMessagingPreferenceClientDto>> GetTutorPreferencesAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<TutorMessagingPreferenceClientDto>(OfflineError()));
        }

        return _apiClient.GetAsync<TutorMessagingPreferenceClientDto>($"/api/v1/automations/tutors/{tutorId}/preferences", cancellationToken);
    }

    /// <summary>Updates tutor messaging preferences.</summary>
    public Task<Result> UpdateTutorPreferencesAsync(Guid tutorId, TutorMessagingPreferenceClientDto dto, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure(OfflineError()));
        }

        return _apiClient.PutAsync($"/api/v1/automations/tutors/{tutorId}/preferences", dto, idempotencyKey: null, cancellationToken);
    }

    /// <summary>Lists recent message jobs.</summary>
    public Task<Result<IReadOnlyList<MessageJobClientDto>>> ListJobsAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<MessageJobClientDto>>(OfflineError()));
        }

        return _apiClient.GetAsync<IReadOnlyList<MessageJobClientDto>>("/api/v1/automations/jobs?take=50", cancellationToken);
    }

    private static Error OfflineError() =>
        new("Automations.Offline", "Automações requer conexão com a API.");
}

/// <summary>Client mirror of message template DTO.</summary>
public sealed class MessageTemplateClientDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string? Subject { get; init; }
    public string Body { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

/// <summary>Client mirror of message job DTO.</summary>
public sealed class MessageJobClientDto
{
    public Guid Id { get; init; }
    public string TemplateCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int AttemptCount { get; init; }
    public string? LastError { get; init; }
    public DateTimeOffset NextAttemptAt { get; init; }
    public IReadOnlyList<JobAttemptLogClientDto> AttemptLogs { get; init; } = Array.Empty<JobAttemptLogClientDto>();
}

/// <summary>Client mirror of attempt log line.</summary>
public sealed class AutomationsSettingsClientDto
{
    public string TimeZoneId { get; set; } = "America/Sao_Paulo";
    public string BusinessStart { get; set; } = "08:00";
    public string BusinessEnd { get; set; } = "18:00";
    public List<string> BusinessDays { get; set; } = new();
}

/// <summary>Client mirror of tutor messaging preferences.</summary>
public sealed class TutorMessagingPreferenceClientDto
{
    public Guid TutorId { get; set; }
    public bool WhatsAppEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
}

/// <summary>Client mirror of attempt log line.</summary>
public sealed class JobAttemptLogClientDto
{
    public int AttemptNumber { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public DateTimeOffset FinishedAt { get; init; }
}
