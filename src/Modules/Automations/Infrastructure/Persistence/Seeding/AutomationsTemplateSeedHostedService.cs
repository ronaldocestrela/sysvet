using Automations.Domain.Entities;
using Automations.Domain.Enums;
using Automations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automations.Infrastructure.Persistence.Seeding;

/// <summary>
/// Ensures default grooming WhatsApp templates exist when the tenant schema is already migrated.
/// Does not apply migrations or throw on unreachable databases so API liveness stays independent of seed.
/// </summary>
public sealed class AutomationsTemplateSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationsTemplateSeedHostedService> _logger;

    public AutomationsTemplateSeedHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutomationsTemplateSeedHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AutomationsDbContext>();

            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogDebug("Skipping Automations template seed: database is not reachable.");
                return;
            }

            var pendingMigrations = await db.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                _logger.LogDebug(
                    "Skipping Automations template seed: {Count} pending migration(s).",
                    pendingMigrations.Count());
                return;
            }

            var templates = scope.ServiceProvider.GetRequiredService<IMessageTemplateRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IAutomationsUnitOfWork>();

            await EnsureTemplateAsync(templates, "grooming.started", "Olá {{TutorName}}, o banho do {{PetName}} foi iniciado.");
            await EnsureTemplateAsync(templates, "grooming.ready", "Olá {{TutorName}}, o {{PetName}} está pronto para retirada!");
            await uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Automations grooming templates seeded when missing.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automations template seeding failed.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task EnsureTemplateAsync(
        IMessageTemplateRepository repository,
        string code,
        string body)
    {
        if (await repository.GetByCodeAndChannelAsync(code, MessageChannel.WhatsApp, CancellationToken.None) is not null)
        {
            return;
        }

        var created = MessageTemplate.Create(code, MessageChannel.WhatsApp, body);
        if (created.IsSuccess)
        {
            repository.Add(created.Value);
        }
    }
}
