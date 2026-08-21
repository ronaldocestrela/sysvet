using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Sync;

/// <summary>
/// Worker de sincronização em segundo plano (roda nos clientes PWA e MAUI).
/// Processa a fila local FIFO e envia para a API.
/// </summary>
public class SyncBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncBackgroundWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public SyncBackgroundWorker(IServiceProvider serviceProvider, ILogger<SyncBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncBackgroundWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado durante a sincronização.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
        var syncClient = scope.ServiceProvider.GetRequiredService<ISyncHttpClient>();

        // 1. PUSH (Enviar do SQLite Local para o SQL Server)
        // Obter até 50 mensagens não processadas, ordenadas por CreatedAt (FIFO)
        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.Error == null) // Apenas as sem erros permanentes
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Any())
        {
            _logger.LogInformation($"Processando {pendingMessages.Count} mensagens da fila Outbox...");

            // Tenta enviar o lote.
            // A API vai processar com "Stop-on-First-Error" e retornar 200 se sucesso total.
            var success = await syncClient.PushAsync(pendingMessages, cancellationToken);

            if (success)
            {
                // Marca como processado
                foreach (var msg in pendingMessages)
                {
                    msg.ProcessedAt = DateTimeOffset.UtcNow;
                }
                
                // Ou alternativamente, deletar: dbContext.OutboxMessages.RemoveRange(pendingMessages);
                
                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Lote de sincronização enviado com sucesso.");
            }
            else
            {
                _logger.LogWarning("Falha ao enviar lote de sincronização. Retentando no próximo ciclo.");
                // O backoff exponencial pode ser controlado aumentando o delay ou via Polly no HttpClient.
            }
        }
        
        // 2. PULL (Baixar novidades do Servidor para o SQLite Local)
        // Isso será implementado posteriormente.
    }
}
