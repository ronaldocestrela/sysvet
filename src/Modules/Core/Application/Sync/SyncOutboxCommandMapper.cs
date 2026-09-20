using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Pets.Commands;
using Core.Application.Tutors.Commands;
using Core.Domain;

namespace Core.Application.Sync;

/// <summary>
/// Deserializes outbox payloads into MediatR commands and injects idempotency keys from the outbox id.
/// </summary>
internal static class SyncOutboxCommandMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Maps a sync message to a command instance ready for MediatR.
    /// </summary>
    public static object? MapToCommand(SyncOutboxMessageDto message)
    {
        var type = message.Type switch
        {
            "RegisterTutorCommand" => nameof(CreateTutorCommand),
            _ => message.Type
        };

        return type switch
        {
            nameof(CreateTutorCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CreateTutorCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(UpdateTutorCommand) => WithIdempotency(
                JsonSerializer.Deserialize<UpdateTutorCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(DeleteTutorCommand) => WithIdempotency(
                JsonSerializer.Deserialize<DeleteTutorCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CreatePetCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CreatePetCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(UpdatePetCommand) => WithIdempotency(
                JsonSerializer.Deserialize<UpdatePetCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(DeletePetCommand) => WithIdempotency(
                JsonSerializer.Deserialize<DeletePetCommand>(message.Payload, JsonOptions),
                message.Id),
            _ => null
        };
    }

    private static CreateTutorCommand? WithIdempotency(CreateTutorCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static UpdateTutorCommand? WithIdempotency(UpdateTutorCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static DeleteTutorCommand? WithIdempotency(DeleteTutorCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CreatePetCommand? WithIdempotency(CreatePetCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static UpdatePetCommand? WithIdempotency(UpdatePetCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static DeletePetCommand? WithIdempotency(DeletePetCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    /// <summary>
    /// Classifies domain errors that should not be retried by the client worker.
    /// </summary>
    public static bool IsPermanentFailure(Error error)
    {
        if (error.Code.Contains("Validation", StringComparison.Ordinal))
        {
            return true;
        }

        return error.Code switch
        {
            var code when code.EndsWith("NotFound", StringComparison.Ordinal) => true,
            var code when code.Contains("Duplicate", StringComparison.Ordinal) => true,
            var code when code.Contains("Invalid", StringComparison.Ordinal) => true,
            var code when code.Contains("Forbidden", StringComparison.Ordinal) => true,
            var code when code.Contains("AlreadyDeleted", StringComparison.Ordinal) => true,
            "Order.InsufficientStock" => true,
            "ProductBalance.InsufficientFunds" => true,
            "Order.DiscountExceedsProfileLimit" => true,
            "Order.ReturnExceedsRemainingQuantity" => true,
            "Order.ReturnNotAllowed" => true,
            "Package.InsufficientBalance" => true,
            "Package.ServiceMismatch" => true,
            "Package.PetRequired" => true,
            "Package.PetMismatch" => true,
            "Package.ReturnAfterConsumption" => true,
            "Package.NotFound" => true,
            _ => false
        };
    }
}
