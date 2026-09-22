using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Application.Messaging;
using Core.Application.Sync;
using Core.Domain;
using MediatR;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Clinical.Commands;
using Veterinary.Application.MedicalRecords.Commands;
using Veterinary.Application.Quotes.Commands;
using Veterinary.Application.Hospitalizations.Commands;
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
            nameof(SetFollowUpOnCommand) => WithIdempotency(
                JsonSerializer.Deserialize<SetFollowUpOnCommand>(message.Payload, JsonOptions),
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
            nameof(CreateClinicalQuoteCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CreateClinicalQuoteCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(ReplaceClinicalQuoteItemsCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ReplaceClinicalQuoteItemsCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(SendClinicalQuoteCommand) => WithIdempotency(
                JsonSerializer.Deserialize<SendClinicalQuoteCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(ApproveClinicalQuoteCommand) => WithIdempotency(
                JsonSerializer.Deserialize<ApproveClinicalQuoteCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(RejectClinicalQuoteCommand) => WithIdempotency(
                JsonSerializer.Deserialize<RejectClinicalQuoteCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(AdmitPetCommand) => WithIdempotency(
                JsonSerializer.Deserialize<AdmitPetCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(DischargePetCommand) => WithIdempotency(
                JsonSerializer.Deserialize<DischargePetCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(TransferHospitalizationBedCommand) => WithIdempotency(
                JsonSerializer.Deserialize<TransferHospitalizationBedCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(CreateMedicationOrderCommand) => WithIdempotency(
                JsonSerializer.Deserialize<CreateMedicationOrderCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(AdministerMedicationCommand) => WithIdempotency(
                JsonSerializer.Deserialize<AdministerMedicationCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(SkipMedicationCommand) => WithIdempotency(
                JsonSerializer.Deserialize<SkipMedicationCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(AddHospitalizationProgressNoteCommand) => WithIdempotency(
                JsonSerializer.Deserialize<AddHospitalizationProgressNoteCommand>(message.Payload, JsonOptions),
                message.Id),
            nameof(AddHospitalProcedureCommand) => WithIdempotency(
                JsonSerializer.Deserialize<AddHospitalProcedureCommand>(message.Payload, JsonOptions),
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
            case SetFollowUpOnCommand setFollowUp:
                return await _mediator.Send(setFollowUp, cancellationToken);
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
            case CreateClinicalQuoteCommand createQuote:
            {
                var quoteResult = await _mediator.Send(createQuote, cancellationToken);
                return quoteResult.IsSuccess ? Result.Success() : Result.Failure(quoteResult.Error);
            }
            case ReplaceClinicalQuoteItemsCommand replaceQuoteItems:
                return await _mediator.Send(replaceQuoteItems, cancellationToken);
            case SendClinicalQuoteCommand sendQuote:
                return await _mediator.Send(sendQuote, cancellationToken);
            case ApproveClinicalQuoteCommand approveQuote:
                return await _mediator.Send(approveQuote, cancellationToken);
            case RejectClinicalQuoteCommand rejectQuote:
                return await _mediator.Send(rejectQuote, cancellationToken);
            case AdmitPetCommand admitPet:
            {
                var admitResult = await _mediator.Send(admitPet, cancellationToken);
                return admitResult.IsSuccess ? Result.Success() : Result.Failure(admitResult.Error);
            }
            case DischargePetCommand dischargePet:
            {
                var dischargeResult = await _mediator.Send(dischargePet, cancellationToken);
                return dischargeResult.IsSuccess ? Result.Success() : Result.Failure(dischargeResult.Error);
            }
            case TransferHospitalizationBedCommand transferBed:
                return await _mediator.Send(transferBed, cancellationToken);
            case CreateMedicationOrderCommand medicationOrder:
            {
                var orderResult = await _mediator.Send(medicationOrder, cancellationToken);
                return orderResult.IsSuccess ? Result.Success() : Result.Failure(orderResult.Error);
            }
            case AdministerMedicationCommand administer:
                return await _mediator.Send(administer, cancellationToken);
            case SkipMedicationCommand skip:
                return await _mediator.Send(skip, cancellationToken);
            case AddHospitalizationProgressNoteCommand progressNote:
            {
                var noteResult = await _mediator.Send(progressNote, cancellationToken);
                return noteResult.IsSuccess ? Result.Success() : Result.Failure(noteResult.Error);
            }
            case AddHospitalProcedureCommand procedure:
            {
                var procResult = await _mediator.Send(procedure, cancellationToken);
                return procResult.IsSuccess ? Result.Success() : Result.Failure(procResult.Error);
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

    private static SetFollowUpOnCommand? WithIdempotency(SetFollowUpOnCommand? command, Guid idempotencyKey) =>
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

    private static CreateClinicalQuoteCommand? WithIdempotency(CreateClinicalQuoteCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey, QuoteId = command.QuoteId == Guid.Empty ? idempotencyKey : command.QuoteId };

    private static ReplaceClinicalQuoteItemsCommand? WithIdempotency(ReplaceClinicalQuoteItemsCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static SendClinicalQuoteCommand? WithIdempotency(SendClinicalQuoteCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static ApproveClinicalQuoteCommand? WithIdempotency(ApproveClinicalQuoteCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static RejectClinicalQuoteCommand? WithIdempotency(RejectClinicalQuoteCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static AdmitPetCommand? WithIdempotency(AdmitPetCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey, HospitalizationId = command.HospitalizationId == Guid.Empty ? idempotencyKey : command.HospitalizationId };

    private static DischargePetCommand? WithIdempotency(DischargePetCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static TransferHospitalizationBedCommand? WithIdempotency(TransferHospitalizationBedCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static CreateMedicationOrderCommand? WithIdempotency(CreateMedicationOrderCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey, OrderId = command.OrderId == Guid.Empty ? idempotencyKey : command.OrderId };

    private static AdministerMedicationCommand? WithIdempotency(AdministerMedicationCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static SkipMedicationCommand? WithIdempotency(SkipMedicationCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey };

    private static AddHospitalizationProgressNoteCommand? WithIdempotency(AddHospitalizationProgressNoteCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey, NoteId = command.NoteId == Guid.Empty ? idempotencyKey : command.NoteId };

    private static AddHospitalProcedureCommand? WithIdempotency(AddHospitalProcedureCommand? command, Guid idempotencyKey) =>
        command is null ? null : command with { IdempotencyKey = idempotencyKey, ProcedureId = command.ProcedureId == Guid.Empty ? idempotencyKey : command.ProcedureId };
}
