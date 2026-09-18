using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Clinical.Commands;
using Veterinary.Application.MedicalRecords.Commands;
using Veterinary.Application.Vaccines.Commands;

namespace Veterinary.Infrastructure.Sync;

/// <summary>
/// Maps veterinary outbox payloads to MediatR commands during sync push.
/// </summary>
public sealed class VeterinarySyncPushHandler : ISyncPushHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public VeterinarySyncPushHandler(IMediator mediator) => _mediator = mediator;

    /// <inheritdoc />
    public object? TryMapCommand(SyncOutboxMessageDto message)
    {
        return message.Type switch
        {
            nameof(ScheduleAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ScheduleAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(ConfirmAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ConfirmAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(StartAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<StartAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CompleteAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CompleteAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(MarkNoShowAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<MarkNoShowAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CancelAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CancelAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RescheduleAppointmentCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RescheduleAppointmentCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CreateMedicalRecordCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CreateMedicalRecordCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(UpdateAnamnesisCommand) => WithIdempotency(
                JsonSerializer.Deserialize<UpdateAnamnesisCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RecordVitalSignsCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RecordVitalSignsCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(AddEvolutionNoteCommand) => WithIdempotency(
                JsonSerializer.Deserialize<AddEvolutionNoteCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(SetDiagnosisCommand) => WithIdempotency(
                JsonSerializer.Deserialize<SetDiagnosisCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(SetConductCommand) => WithIdempotency(
                JsonSerializer.Deserialize<SetConductCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(FinalizeMedicalRecordCommand) => WithIdempotency(
                JsonSerializer.Deserialize<FinalizeMedicalRecordCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RequestClinicalExamCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RequestClinicalExamCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CompleteClinicalExamCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CompleteClinicalExamCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CreateIssuedPrescriptionCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CreateIssuedPrescriptionCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(IssuePrescriptionCommand) => WithIdempotency(
                JsonSerializer.Deserialize<IssuePrescriptionCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RegisterVaccineDoseCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RegisterVaccineDoseCommand>(message.Payload, JsonOptions),
                message.Id),
            _ => null
        };
    }

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(object command, CancellationToken cancellationToken)
    {
        switch (command)
        {
            case ScheduleAppointmentCommand schedule:
            {
                var result = await _mediator.Send(schedule, cancellationToken);
                return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
            }
            case ConfirmAppointmentCommand confirm:
                return await _mediator.Send(confirm, cancellationToken);
            case StartAppointmentCommand start:
                return await _mediator.Send(start, cancellationToken);
            case CompleteAppointmentCommand complete:
                return await _mediator.Send(complete, cancellationToken);
            case MarkNoShowAppointmentCommand noShow:
                return await _mediator.Send(noShow, cancellationToken);
            case CancelAppointmentCommand cancel:
                return await _mediator.Send(cancel, cancellationToken);
            case RescheduleAppointmentCommand reschedule:
                return await _mediator.Send(reschedule, cancellationToken);
            case CreateMedicalRecordCommand createRecord:
            {
                var createResult = await _mediator.Send(createRecord, cancellationToken);
                return createResult.IsSuccess ? Result.Success() : Result.Failure(createResult.Error);
            }
            case UpdateAnamnesisCommand updateAnamnesis:
                return await _mediator.Send(updateAnamnesis, cancellationToken);
            case RecordVitalSignsCommand recordVitals:
                return await _mediator.Send(recordVitals, cancellationToken);
            case AddEvolutionNoteCommand addEvolution:
            {
                var evolutionResult = await _mediator.Send(addEvolution, cancellationToken);
                return evolutionResult.IsSuccess ? Result.Success() : Result.Failure(evolutionResult.Error);
            }
            case SetDiagnosisCommand setDiagnosis:
                return await _mediator.Send(setDiagnosis, cancellationToken);
            case SetConductCommand setConduct:
                return await _mediator.Send(setConduct, cancellationToken);
            case FinalizeMedicalRecordCommand finalize:
                return await _mediator.Send(finalize, cancellationToken);
            case RequestClinicalExamCommand requestExam:
            {
                var examResult = await _mediator.Send(requestExam, cancellationToken);
                return examResult.IsSuccess ? Result.Success() : Result.Failure(examResult.Error);
            }
            case CompleteClinicalExamCommand completeExam:
                return await _mediator.Send(completeExam, cancellationToken);
            case CreateIssuedPrescriptionCommand createPrescription:
            {
                var prescriptionResult = await _mediator.Send(createPrescription, cancellationToken);
                return prescriptionResult.IsSuccess ? Result.Success() : Result.Failure(prescriptionResult.Error);
            }
            case IssuePrescriptionCommand issuePrescription:
                return await _mediator.Send(issuePrescription, cancellationToken);
            case RegisterVaccineDoseCommand registerVaccine:
            {
                var vaccineResult = await _mediator.Send(registerVaccine, cancellationToken);
                return vaccineResult.IsSuccess ? Result.Success() : Result.Failure(vaccineResult.Error);
            }
            default:
                return Result.Failure(new Error("Sync.HandlerMismatch", "Not a veterinary sync command."));
        }
    }

    private static ScheduleAppointmentCommand? WithIdempotency(ScheduleAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static ConfirmAppointmentCommand? WithIdempotency(ConfirmAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static StartAppointmentCommand? WithIdempotency(StartAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CompleteAppointmentCommand? WithIdempotency(CompleteAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static MarkNoShowAppointmentCommand? WithIdempotency(MarkNoShowAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CancelAppointmentCommand? WithIdempotency(CancelAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RescheduleAppointmentCommand? WithIdempotency(RescheduleAppointmentCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CreateMedicalRecordCommand? WithIdempotency(CreateMedicalRecordCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static UpdateAnamnesisCommand? WithIdempotency(UpdateAnamnesisCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RecordVitalSignsCommand? WithIdempotency(RecordVitalSignsCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static AddEvolutionNoteCommand? WithIdempotency(AddEvolutionNoteCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static SetDiagnosisCommand? WithIdempotency(SetDiagnosisCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static SetConductCommand? WithIdempotency(SetConductCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static FinalizeMedicalRecordCommand? WithIdempotency(FinalizeMedicalRecordCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RequestClinicalExamCommand? WithIdempotency(RequestClinicalExamCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CompleteClinicalExamCommand? WithIdempotency(CompleteClinicalExamCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CreateIssuedPrescriptionCommand? WithIdempotency(CreateIssuedPrescriptionCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static IssuePrescriptionCommand? WithIdempotency(IssuePrescriptionCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RegisterVaccineDoseCommand? WithIdempotency(RegisterVaccineDoseCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey, Id = idempotencyKey };
}
