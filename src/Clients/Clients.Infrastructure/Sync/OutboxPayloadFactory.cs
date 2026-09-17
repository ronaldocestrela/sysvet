namespace Clients.Infrastructure.Sync;

/// <summary>
/// JSON payloads aligned with Core.Application CRM command records for sync push.
/// </summary>
internal static class OutboxPayloadFactory
{
    public static string CreateTutor(Guid id, string name, string email, string cpf, string phone, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Email = email,
            Cpf = cpf,
            Phone = phone,
            IdempotencyKey = idempotencyKey
        });

    public static string UpdateTutor(Guid id, string name, string email, string phone, DateTimeOffset occurredAt, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Email = email,
            Phone = phone,
            OccurredAt = occurredAt,
            IdempotencyKey = idempotencyKey
        });

    public static string DeleteTutor(Guid id, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { Id = id, IdempotencyKey = idempotencyKey });

    public static string CreatePet(Guid id, string name, string species, string breed, string sex, Guid tutorId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            Sex = sex,
            TutorId = tutorId,
            IdempotencyKey = idempotencyKey
        });

    public static string UpdatePet(Guid id, string name, string species, string breed, string sex, DateTimeOffset occurredAt, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            Sex = sex,
            OccurredAt = occurredAt,
            IdempotencyKey = idempotencyKey
        });

    public static string DeletePet(Guid id, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { Id = id, IdempotencyKey = idempotencyKey });
}
