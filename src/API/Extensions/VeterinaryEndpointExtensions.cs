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

        recordsGroup.MapPatch("/{id:guid}/follow-up", async (Guid id, HttpContext httpContext, [FromBody] FollowUpRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.SetFollowUpOnCommand(id, body.FollowUpOn, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

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

        var wardGroup = builder.MapGroup("/api/v1/ward-units").RequireAuthorization().WithTags("Veterinary", "WardUnits");
        wardGroup.MapGet("/", async (IMediator mediator, [FromQuery] bool activeOnly = true) =>
            (await mediator.Send(new Veterinary.Application.WardUnits.Commands.ListWardUnitsQuery(activeOnly))).ToHttpResult());
        wardGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.WardUnits.Commands.GetWardUnitByIdQuery(id))).ToHttpResult());
        wardGroup.MapPost("/", async (HttpContext httpContext, [FromBody] CreateWardUnitRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.WardUnits.Commands.CreateWardUnitCommand(body.Name, body.Beds, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        wardGroup.MapPut("/{id:guid}", async (Guid id, HttpContext httpContext, [FromBody] UpdateWardUnitRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.WardUnits.Commands.UpdateWardUnitCommand(id, body.Name, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        wardGroup.MapPut("/{id:guid}/beds", async (Guid id, HttpContext httpContext, [FromBody] ReplaceWardBedsRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.WardUnits.Commands.ReplaceWardUnitBedsCommand(id, body.Beds, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        wardGroup.MapPost("/{id:guid}/deactivate", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.WardUnits.Commands.DeactivateWardUnitCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var hospGroup = builder.MapGroup("/api/v1/hospitalizations").RequireAuthorization().WithTags("Veterinary", "Hospitalizations");
        hospGroup.MapGet("/execution-map", async (IMediator mediator, [FromQuery] DateOnly? date) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.GetExecutionMapQuery(date))).ToHttpResult());
        hospGroup.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.ListActiveHospitalizationsQuery())).ToHttpResult());
        hospGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.GetHospitalizationByIdQuery(id))).ToHttpResult());
        hospGroup.MapPost("/", async (HttpContext httpContext, [FromBody] AdmitPetRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.AdmitPetCommand(
                body.PetId,
                body.VeterinarianId,
                body.BedId,
                body.Reason,
                body.HospitalizationId,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        hospGroup.MapPost("/{id:guid}/discharge", async (Guid id, HttpContext httpContext, IMediator mediator) =>
        {
            var result = await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.DischargePetCommand(id, EndpointIdempotency.ReadKey(httpContext)));
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });
        hospGroup.MapPost("/{id:guid}/transfer", async (Guid id, HttpContext httpContext, [FromBody] TransferBedRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.TransferHospitalizationBedCommand(id, body.NewBedId, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        hospGroup.MapPost("/{id:guid}/medication-orders", async (Guid id, HttpContext httpContext, [FromBody] CreateMedicationOrderRequest body, IMediator mediator) =>
        {
            var times = body.DailyTimes?.ToList() ?? [];
            if (times.Count == 0 && body.DailyTimeStrings is { Count: > 0 })
            {
                times = body.DailyTimeStrings.Select(TimeOnly.Parse).ToList();
            }

            return (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.CreateMedicationOrderCommand(
                id,
                body.MedicationName,
                body.Dose,
                body.Route,
                times,
                body.StartsOn,
                body.EndsOn,
                body.OrderId,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult();
        });
        hospGroup.MapPost("/{id:guid}/administrations/{administrationId:guid}/administer", async (Guid id, Guid administrationId, HttpContext httpContext, [FromBody] AdministrationNotesRequest? body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.AdministerMedicationCommand(
                id,
                administrationId,
                body?.Notes,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        hospGroup.MapPost("/{id:guid}/administrations/{administrationId:guid}/skip", async (Guid id, Guid administrationId, HttpContext httpContext, [FromBody] AdministrationNotesRequest? body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.SkipMedicationCommand(
                id,
                administrationId,
                body?.Notes,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        hospGroup.MapPost("/{id:guid}/progress-notes", async (Guid id, HttpContext httpContext, [FromBody] AddProgressNoteRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.AddHospitalizationProgressNoteCommand(
                id,
                body.Text,
                body.NoteId,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        hospGroup.MapPost("/{id:guid}/procedures", async (Guid id, HttpContext httpContext, [FromBody] AddHospitalProcedureRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.AddHospitalProcedureCommand(
                id,
                body.Name,
                body.VeterinarianId,
                body.PerformedAt,
                body.Notes,
                body.ProcedureId,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

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

        petsGroup.MapGet("/{petId:guid}/quotes", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.ListClinicalQuotesByPetQuery(petId))).ToHttpResult());

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
        group.MapGet("/{id:guid}/quotes", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.ListClinicalQuotesByAppointmentQuery(id))).ToHttpResult());
        group.MapPost("/{id:guid}/quotes", async (Guid id, HttpContext httpContext, [FromBody] CreateClinicalQuoteRequest? body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.CreateClinicalQuoteCommand(id, body?.Notes, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var quotesGroup = builder.MapGroup("/api/v1/clinical-quotes").RequireAuthorization().WithTags("Veterinary", "ClinicalQuotes");
        quotesGroup.MapGet("/pending-conversions", async (IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.ListPendingQuoteConversionsQuery())).ToHttpResult());
        quotesGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.GetClinicalQuoteByIdQuery(id))).ToHttpResult());
        quotesGroup.MapPut("/{id:guid}/items", async (Guid id, HttpContext httpContext, [FromBody] ReplaceClinicalQuoteItemsRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.ReplaceClinicalQuoteItemsCommand(id, body.Items, body.Notes, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        quotesGroup.MapPost("/{id:guid}/send", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.SendClinicalQuoteCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        quotesGroup.MapPost("/{id:guid}/approve", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.ApproveClinicalQuoteCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());
        quotesGroup.MapPost("/{id:guid}/reject", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.Quotes.Commands.RejectClinicalQuoteCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

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

    public record FollowUpRequest(DateOnly? FollowUpOn);

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

    /// <summary>Clinical quote create body.</summary>
    public record CreateClinicalQuoteRequest(string? Notes);

    /// <summary>Replace clinical quote lines body.</summary>
    public record ReplaceClinicalQuoteItemsRequest(
        IReadOnlyList<Veterinary.Application.Quotes.Commands.ClinicalQuoteLineInput> Items,
        string? Notes);

    /// <summary>Ward unit create body.</summary>
    public record CreateWardUnitRequest(string Name, IReadOnlyList<Veterinary.Application.WardUnits.Commands.WardBedInput> Beds);

    /// <summary>Ward unit rename body.</summary>
    public record UpdateWardUnitRequest(string Name);

    /// <summary>Replace ward beds body.</summary>
    public record ReplaceWardBedsRequest(IReadOnlyList<Veterinary.Application.WardUnits.Commands.WardBedInput> Beds);

    /// <summary>Admit pet body.</summary>
    public record AdmitPetRequest(Guid PetId, Guid VeterinarianId, Guid BedId, string Reason, Guid HospitalizationId = default);

    /// <summary>Transfer bed body.</summary>
    public record TransferBedRequest(Guid NewBedId);

    /// <summary>Medication order body.</summary>
    public record CreateMedicationOrderRequest(
        string MedicationName,
        string Dose,
        string Route,
        IReadOnlyList<TimeOnly>? DailyTimes,
        IReadOnlyList<string>? DailyTimeStrings,
        DateOnly StartsOn,
        DateOnly EndsOn,
        Guid OrderId = default);

    /// <summary>Administration notes body.</summary>
    public record AdministrationNotesRequest(string? Notes);

    /// <summary>Progress note body.</summary>
    public record AddProgressNoteRequest(string Text, Guid NoteId = default);

    /// <summary>Inpatient procedure body.</summary>
    public record AddHospitalProcedureRequest(
        string Name,
        Guid VeterinarianId,
        DateTimeOffset PerformedAt,
        string? Notes,
        Guid ProcedureId = default);
}
