using Automations.Domain.Entities;
using Automations.Domain.Enums;

namespace Automations.Domain.Repositories;

/// <summary>
/// Persistence port for message templates.
/// </summary>
public interface IMessageTemplateRepository
{
    void Add(MessageTemplate template);

    Task<MessageTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MessageTemplate?> GetByCodeAndChannelAsync(string code, MessageChannel channel, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageTemplate>> ListAsync(CancellationToken cancellationToken = default);
}
