using Automations.Domain.Enums;

namespace Automations.Application.Templates.Dtos;

/// <summary>
/// Template DTO for API responses.
/// </summary>
public sealed record MessageTemplateDto(
    Guid Id,
    string Code,
    string Channel,
    string? Subject,
    string Body,
    bool IsActive);
