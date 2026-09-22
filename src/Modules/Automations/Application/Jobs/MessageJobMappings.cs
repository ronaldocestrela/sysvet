using Automations.Application.Jobs.Dtos;
using Automations.Domain.Entities;

namespace Automations.Application.Jobs;

/// <summary>
/// Maps domain jobs to API DTOs.
/// </summary>
public static class MessageJobMappings
{
    /// <summary>
    /// Converts a job aggregate to a DTO including attempt history.
    /// </summary>
    public static MessageJobDto ToDto(MessageJob job) =>
        new(
            job.Id,
            job.TenantId,
            job.Channel.ToString(),
            job.TemplateCode,
            job.Status.ToString(),
            job.AttemptCount,
            job.MaxAttempts,
            job.NextAttemptAt,
            job.LastError,
            job.SourceType,
            job.SourceId,
            job.AttemptLogs.Select(a => new JobAttemptLogDto(
                a.AttemptNumber,
                a.Outcome.ToString(),
                a.Detail,
                a.StartedAt,
                a.FinishedAt)).ToList());
}
