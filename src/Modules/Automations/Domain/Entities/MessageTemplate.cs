using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Tenant-scoped message template for a specific channel and business code.
/// </summary>
public sealed class MessageTemplate : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public MessageChannel Channel { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private MessageTemplate() { }

    /// <summary>
    /// Creates a new active template.
    /// </summary>
    public static Result<MessageTemplate> Create(
        string code,
        MessageChannel channel,
        string body,
        string? subject = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<MessageTemplate>(ErrorCodes.Template.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure<MessageTemplate>(ErrorCodes.Template.InvalidBody);
        }

        return Result.Success(new MessageTemplate
        {
            Id = id ?? Guid.NewGuid(),
            Code = code.Trim(),
            Channel = channel,
            Body = body,
            Subject = string.IsNullOrWhiteSpace(subject) ? null : subject.Trim(),
            IsActive = true
        });
    }

    /// <summary>
    /// Updates body, subject, and active flag.
    /// </summary>
    public Result Update(string body, string? subject, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure(ErrorCodes.Template.InvalidBody);
        }

        Body = body;
        Subject = string.IsNullOrWhiteSpace(subject) ? null : subject.Trim();
        IsActive = isActive;
        return Result.Success();
    }
}
