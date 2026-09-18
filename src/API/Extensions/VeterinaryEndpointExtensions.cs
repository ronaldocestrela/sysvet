using Core.Domain;
using Core.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Appointments.Queries;
using Veterinary.Application.ScheduleSlots.Commands;
using Veterinary.Application.ScheduleSlots.Queries;

namespace API.Extensions;

/// <summary>
/// Veterinary module minimal API endpoints.
/// </summary>
public static class VeterinaryEndpointExtensions
{
    /// <summary>
    /// Maps appointments, schedule slots, vaccines, and hospitalization routes.
    /// </summary>
    public static IEndpointRouteBuilder MapVeterinaryEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/appointments").RequireAuthorization().WithTags("Veterinary", "Appointments");

        group.MapPost("/", async (HttpContext httpContext, [FromBody] ScheduleAppointmentCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            var commandWithKey = command with { IdempotencyKey = key };
            return (await mediator.Send(commandWithKey)).ToHttpResult();
        });

        group.MapGet("/daily", async ([FromQuery] Guid? veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetDailyScheduleQuery(veterinarianId, date))).ToHttpResult());

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetAppointmentByIdQuery(id))).ToHttpResult());

        group.MapPut("/{id:guid}/reschedule", async (Guid id, HttpContext httpContext, [FromBody] RescheduleRequest request, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new RescheduleAppointmentCommand(id, request.NewDate, IdempotencyKey: key))).ToHttpResult();
        });

        group.MapPost("/{id:guid}/confirm", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new ConfirmAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/start", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new StartAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/complete", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CompleteAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/no-show", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new MarkNoShowAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CancelAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/records", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.CreateMedicalRecordCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var slotsGroup = builder.MapGroup("/api/v1/schedule-slots").RequireAuthorization().WithTags("Veterinary", "ScheduleSlots");

        slotsGroup.MapGet("/", async ([FromQuery] Guid veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetScheduleSlotsQuery(veterinarianId, date))).ToHttpResult());

        slotsGroup.MapPost("/availability", async (HttpContext httpContext, [FromBody] DefineDailyAvailabilityCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command with { IdempotencyKey = key })).ToHttpResult();
        });

        slotsGroup.MapPost("/{id:guid}/block", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new BlockScheduleSlotCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        slotsGroup.MapPost("/{id:guid}/unblock", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new UnblockScheduleSlotCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var recordsGroup = builder.MapGroup("/api/v1/medical-records").RequireAuthorization().WithTags("Veterinary", "MedicalRecords");

        recordsGroup.MapGet("/by-pet/{petId:guid}", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Queries.GetPetClinicalTimelineQuery(petId))).ToHttpResult());

        recordsGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Queries.GetMedicalRecordByIdQuery(id))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/anamnesis", async (Guid id, HttpContext httpContext, [FromBody] AnamnesisRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.UpdateAnamnesisCommand(id, body.Anamnesis, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/vitals", async (Guid id, HttpContext httpContext, [FromBody] VitalSignsRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.RecordVitalSignsCommand(
                id, body.WeightKg, body.TemperatureC, body.HeartRateBpm, body.RespiratoryRateBpm,
                body.MucousMembranes, body.CapillaryRefillTime, body.MeasuredAt, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/diagnosis", async (Guid id, HttpContext httpContext, [FromBody] DiagnosisRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.SetDiagnosisCommand(id, body.Diagnosis, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/conduct", async (Guid id, HttpContext httpContext, [FromBody] ConductRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.SetConductCommand(id, body.Conduct, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPost("/{id:guid}/evolution", async (Guid id, HttpContext httpContext, [FromBody] EvolutionNoteRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.AddEvolutionNoteCommand(id, body.Text, body.NoteId, body.RecordedAt, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPost("/{id:guid}/finalize", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.FinalizeMedicalRecordCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var petsGroup = builder.MapGroup("/api/v1/pets").RequireAuthorization().WithTags("Veterinary", "Pets (Veterinary)");

        petsGroup.MapGet("/{petId:guid}/medical-records", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Queries.GetPetClinicalTimelineQuery(petId))).ToHttpResult());

        petsGroup.MapGet("/{petId:guid}/vaccines", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.ListVaccineDosesByPetQuery(petId))).ToHttpResult());

        petsGroup.MapGet("/{petId:guid}/vaccination-card", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.GetVaccinationCardQuery(petId))).ToHttpResult());

        petsGroup.MapPost("/{petId:guid}/vaccines", async (Guid petId, HttpContext httpContext, [FromBody] RegisterVaccineDoseRequest body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            var command = new Veterinary.Application.Vaccines.Commands.RegisterVaccineDoseCommand(
                petId,
                body.Name,
                body.BatchNumber,
                body.AppliedAt,
                body.NextDueDate,
                body.ProtocolDoseId,
                body.Id ?? Guid.Empty,
                key);

            if (petId != command.PetId)
            {
                return Result.Failure<Guid>(new Error("Vaccine.MismatchedPetId", "URL petId does not match command.")).ToHttpResult();
            }

            return (await mediator.Send(command)).ToHttpResult();
        });

        var vaccineProtocolsGroup = builder.MapGroup("/api/v1/vaccine-protocols").RequireAuthorization().WithTags("Veterinary", "VaccineProtocols");
        vaccineProtocolsGroup.MapGet("/", async (IMediator mediator, [FromQuery] PetSpecies? species, [FromQuery] bool activeOnly = true) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.ListVaccineProtocolsQuery(species, activeOnly))).ToHttpResult());
        vaccineProtocolsGroup.MapPost("/", async (HttpContext httpContext, [FromBody] CreateVaccineProtocolRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.CreateVaccineProtocolCommand(body.Name, body.Species, body.Doses, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        vaccineProtocolsGroup.MapPut("/{id:guid}", async (Guid id, HttpContext httpContext, [FromBody] UpdateVaccineProtocolRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.UpdateVaccineProtocolCommand(id, body.Name, body.Species, body.Doses, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        vaccineProtocolsGroup.MapPost("/{id:guid}/deactivate", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.DeactivateVaccineProtocolCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        builder.MapGet("/api/v1/vaccine-alerts", async (IMediator mediator, [FromQuery] Veterinary.Application.Vaccines.Dtos.VaccineAlertStatusDto? status, [FromQuery] int horizonDays = 7, [FromQuery] int page = 1, [FromQuery] int pageSize = 50) =>
            (await mediator.Send(new Veterinary.Application.Vaccines.Commands.ListVaccineAlertsQuery(status, horizonDays, page, pageSize))).ToHttpResult())
            .RequireAuthorization()
            .WithTags("Veterinary", "VaccineAlerts");

        var hospGroup = builder.MapGroup("/api/v1/hospitalizations").RequireAuthorization().WithTags("Veterinary", "Hospitalizations");
        hospGroup.MapPost("/", async ([FromBody] Veterinary.Application.Hospitalizations.Commands.AdmitPetCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        hospGroup.MapPost("/{id:guid}/discharge", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.DischargePetCommand(id));
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        hospGroup.MapPost("/{id:guid}/prescriptions/execute", async (Guid id, [FromBody] Veterinary.Application.Hospitalizations.Commands.ExecutePrescriptionCommand command, IMediator mediator) =>
        {
            if (id != command.HospitalizationId)
            {
                return Result.Failure<Guid>(new Error("Hospitalization.MismatchedId", "URL id does not match command.")).ToHttpResult();
            }

            return (await mediator.Send(command)).ToHttpResult();
        });

        var templatesGroup = builder.MapGroup("/api/v1/prescription-templates").RequireAuthorization().WithTags("Veterinary", "PrescriptionTemplates");
        templatesGroup.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.ListPrescriptionTemplatesQuery())).ToHttpResult());
        templatesGroup.MapPost("/", async (HttpContext httpContext, [FromBody] CreatePrescriptionTemplateRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.CreatePrescriptionTemplateCommand(body.Name, body.Species, body.Items, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        templatesGroup.MapPut("/{id:guid}", async (Guid id, HttpContext httpContext, [FromBody] UpdatePrescriptionTemplateRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.UpdatePrescriptionTemplateCommand(id, body.Name, body.Species, body.Items, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        templatesGroup.MapPost("/{id:guid}/deactivate", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.DeactivatePrescriptionTemplateCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var prescriptionsGroup = builder.MapGroup("/api/v1/prescriptions").RequireAuthorization().WithTags("Veterinary", "Prescriptions");
        prescriptionsGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.GetIssuedPrescriptionByIdQuery(id))).ToHttpResult());
        prescriptionsGroup.MapPost("/{id:guid}/issue", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.IssuePrescriptionCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        prescriptionsGroup.MapPut("/{id:guid}/items", async (Guid id, HttpContext httpContext, [FromBody] ReplacePrescriptionItemsRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.ReplaceIssuedPrescriptionItemsCommand(id, body.Items, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapGet("/{id:guid}/prescriptions", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.ListIssuedPrescriptionsByAppointmentQuery(id))).ToHttpResult());
        group.MapPost("/{id:guid}/prescriptions", async (Guid id, HttpContext httpContext, [FromBody] CreateIssuedPrescriptionRequest? body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.CreateIssuedPrescriptionCommand(id, body?.TemplateId, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapGet("/{id:guid}/exams", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.ListClinicalExamsByAppointmentQuery(id))).ToHttpResult());
        group.MapPost("/{id:guid}/exams", async (Guid id, HttpContext httpContext, [FromBody] RequestClinicalExamRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.RequestClinicalExamCommand(id, body.Name, body.Category, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var examsGroup = builder.MapGroup("/api/v1/exams").RequireAuthorization().WithTags("Veterinary", "ClinicalExams");
        examsGroup.MapPost("/{id:guid}/complete", async (Guid id, HttpContext httpContext, [FromBody] CompleteClinicalExamRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.CompleteClinicalExamCommand(id, body.ResultSummary, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        examsGroup.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.CancelClinicalExamCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        petsGroup.MapGet("/{petId:guid}/exams", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.ListClinicalExamsByPetQuery(petId))).ToHttpResult());

        var attachmentsGroup = builder.MapGroup("/api/v1/attachments").RequireAuthorization().WithTags("Veterinary", "ClinicalAttachments");
        attachmentsGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.GetClinicalAttachmentQuery(id))).ToHttpResult());
        attachmentsGroup.MapGet("/{id:guid}/content", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new Veterinary.Application.Clinical.Commands.DownloadClinicalAttachmentQuery(id));
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
        });
        attachmentsGroup.MapDelete("/{id:guid}", async (Guid id, HttpContext httpContext, IMediator mediator) =>
        {
            var result = await mediator.Send(new Veterinary.Application.Clinical.Commands.SoftDeleteClinicalAttachmentCommand(id, EndpointIdempotency.ReadKey(httpContext)));
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        group.MapGet("/{id:guid}/attachments", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Clinical.Commands.ListClinicalAttachmentsByAppointmentQuery(id))).ToHttpResult());
        group.MapPost("/{id:guid}/attachments", async (Guid id, HttpContext httpContext, IMediator mediator) =>
        {
            if (!httpContext.Request.HasFormContentType)
            {
                return Results.BadRequest("Multipart form expected.");
            }

            var form = await httpContext.Request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            if (file is null)
            {
                return Results.BadRequest("Missing file field.");
            }

            Guid? medicalRecordId = form.TryGetValue("medicalRecordId", out var mr) && Guid.TryParse(mr, out var mrId) ? mrId : null;
            Guid? examId = form.TryGetValue("clinicalExamId", out var ex) && Guid.TryParse(ex, out var exId) ? exId : null;

            await using var stream = file.OpenReadStream();
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
            var command = new Veterinary.Application.Clinical.Commands.UploadClinicalAttachmentCommand(
                id,
                medicalRecordId,
                examId,
                file.FileName,
                contentType,
                file.Length,
                stream,
                EndpointIdempotency.ReadKey(httpContext));

            return (await mediator.Send(command)).ToHttpResult();
        });

        return builder;
    }

    /// <summary>
    /// Request body for rescheduling an appointment.
    /// </summary>
    /// <param name="NewDate">New scheduled date and time.</param>
    public record RescheduleRequest(DateTimeOffset NewDate);

    /// <summary>Anamnesis update body.</summary>
    public record AnamnesisRequest(string Anamnesis);

    /// <summary>Vital signs update body.</summary>
    public record VitalSignsRequest(
        decimal WeightKg,
        decimal TemperatureC,
        int? HeartRateBpm,
        int? RespiratoryRateBpm,
        string MucousMembranes,
        string CapillaryRefillTime,
        DateTimeOffset MeasuredAt);

    /// <summary>Diagnosis update body.</summary>
    public record DiagnosisRequest(string Diagnosis);

    /// <summary>Conduct update body.</summary>
    public record ConductRequest(string Conduct);

    /// <summary>Evolution note append body.</summary>
    public record EvolutionNoteRequest(string Text, Guid NoteId = default, DateTimeOffset? RecordedAt = null);

    /// <summary>Template create body.</summary>
    public record CreatePrescriptionTemplateRequest(string Name, string? Species, IReadOnlyList<Veterinary.Application.Clinical.Commands.PrescriptionLineInput> Items);

    /// <summary>Template update body.</summary>
    public record UpdatePrescriptionTemplateRequest(string Name, string? Species, IReadOnlyList<Veterinary.Application.Clinical.Commands.PrescriptionLineInput> Items);

    /// <summary>Draft prescription create body.</summary>
    public record CreateIssuedPrescriptionRequest(Guid? TemplateId);

    /// <summary>Replace prescription lines body.</summary>
    public record ReplacePrescriptionItemsRequest(IReadOnlyList<Veterinary.Application.Clinical.Commands.PrescriptionLineInput> Items);

    /// <summary>Exam request body.</summary>
    public record RequestClinicalExamRequest(string Name, string Category);

    /// <summary>Exam completion body.</summary>
    public record CompleteClinicalExamRequest(string? ResultSummary);

    /// <summary>Vaccine dose registration body.</summary>
    public record RegisterVaccineDoseRequest(
        string Name,
        string BatchNumber,
        DateTimeOffset AppliedAt,
        DateTimeOffset? NextDueDate,
        Guid? ProtocolDoseId,
        Guid? Id);

    /// <summary>Vaccine protocol create body.</summary>
    public record CreateVaccineProtocolRequest(
        string Name,
        PetSpecies Species,
        IReadOnlyList<Veterinary.Application.Vaccines.Dtos.VaccineProtocolDoseInput> Doses);

    /// <summary>Vaccine protocol update body.</summary>
    public record UpdateVaccineProtocolRequest(
        string Name,
        PetSpecies Species,
        IReadOnlyList<Veterinary.Application.Vaccines.Dtos.VaccineProtocolDoseInput> Doses);
}
