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
public sealed class JobAttemptLogClientDto
{
    public int AttemptNumber { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public DateTimeOffset FinishedAt { get; init; }
}
