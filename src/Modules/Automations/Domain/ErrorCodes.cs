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

    public static class Channel
    {
        public static readonly Error SmsNotSupported = new("MessageChannel.SmsNotSupported", "SMS não está disponível nesta versão.");
    }

    public static class Preference
    {
        public static readonly Error InvalidTutor = new("TutorMessagingPreference.InvalidTutor", "TutorId é obrigatório.");
    }

    public static class Settings
    {
        public static readonly Error InvalidBusinessHours = new("AutomationsSettings.InvalidBusinessHours", "Horário comercial inválido.");
    }
}
