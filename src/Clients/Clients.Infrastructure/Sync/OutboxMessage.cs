namespace Clients.Infrastructure.Sync;

/// <summary>
/// Representa uma mensagem (Comando CQRS) pendente de sincronização para a API.
/// Esta entidade existe apenas no banco de dados local do cliente (SQLite).
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// O tipo CLR do comando (ex: "RegisterTutorCommand") para desserialização no servidor.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// O conteúdo serializado do comando em formato JSON.
    /// </summary>
    public string Payload { get; set; } = string.Empty;
    
    /// <summary>
    /// Data/hora em que a operação ocorreu localmente.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Data/hora em que a mensagem foi processada e recebida com sucesso pela API.
    /// Nulo se ainda estiver pendente.
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }
    
    /// <summary>
    /// Mensagem de erro caso o processamento ou envio tenha falhado.
    /// </summary>
    public string? Error { get; set; }
}
