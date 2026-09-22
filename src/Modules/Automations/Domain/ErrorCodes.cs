using Core.Domain;

namespace Automations.Domain;

/// <summary>
/// Standardized error codes for the Automations module.
/// </summary>
public static class ErrorCodes
{
    public static class Template
    {
        public static readonly Error NotFound = new("MessageTemplate.NotFound", "Template de mensagem não encontrado.");
        public static readonly Error InvalidCode = new("MessageTemplate.InvalidCode", "Código do template é obrigatório.");
        public static readonly Error InvalidBody = new("MessageTemplate.InvalidBody", "Corpo do template é obrigatório.");
        public static readonly Error DuplicateCode = new("MessageTemplate.DuplicateCode", "Já existe template para este código e canal.");
        public static readonly Error RenderFailed = new("MessageTemplate.RenderFailed", "Falha ao renderizar template.");
        public static readonly Error UnknownToken = new("MessageTemplate.UnknownToken", "Token desconhecido no template.");
    }

    public static class Job
    {
        public static readonly Error NotFound = new("MessageJob.NotFound", "Job de mensagem não encontrado.");
        public static readonly Error InvalidPayload = new("MessageJob.InvalidPayload", "Payload do job é inválido.");
        public static readonly Error DuplicateIdempotency = new("MessageJob.DuplicateIdempotency", "Job já enfileirado para esta chave de idempotência.");
    }
}
