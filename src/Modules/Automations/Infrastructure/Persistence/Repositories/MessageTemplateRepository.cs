using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Automations.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of <see cref="IMessageTemplateRepository"/>.
/// </summary>
public sealed class MessageTemplateRepository : IMessageTemplateRepository
{
    private readonly AutomationsDbContext _dbContext;

    public MessageTemplateRepository(AutomationsDbContext dbContext) => _dbContext = dbContext;

    public void Add(MessageTemplate template) => _dbContext.MessageTemplates.Add(template);

    public Task<MessageTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.MessageTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<MessageTemplate?> GetByCodeAndChannelAsync(string code, MessageChannel channel, CancellationToken cancellationToken = default) =>
        _dbContext.MessageTemplates.FirstOrDefaultAsync(t => t.Code == code && t.Channel == channel, cancellationToken);

    public async Task<IReadOnlyList<MessageTemplate>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.MessageTemplates.OrderBy(t => t.Code).ToListAsync(cancellationToken);
}
