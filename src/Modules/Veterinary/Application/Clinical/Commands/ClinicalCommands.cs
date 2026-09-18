using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Authorization;
using MediatR;
using Veterinary.Application.Clinical.Dtos;

namespace Veterinary.Application.Clinical.Commands;

/// <summary>Creates a prescription template.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record CreatePrescriptionTemplateCommand(string Name, string? Species, IReadOnlyList<PrescriptionLineInput> Items, Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Updates template metadata and items.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record UpdatePrescriptionTemplateCommand(Guid TemplateId, string Name, string? Species, IReadOnlyList<PrescriptionLineInput> Items, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Deactivates a template.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record DeactivatePrescriptionTemplateCommand(Guid TemplateId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Lists active templates.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record ListPrescriptionTemplatesQuery : IQuery<IReadOnlyList<PrescriptionTemplateDto>>;

/// <summary>Creates a draft issued prescription for an appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record CreateIssuedPrescriptionCommand(Guid AppointmentId, Guid? TemplateId, Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Replaces draft prescription lines.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record ReplaceIssuedPrescriptionItemsCommand(Guid PrescriptionId, IReadOnlyList<PrescriptionLineInput> Items, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Issues a draft prescription.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record IssuePrescriptionCommand(Guid PrescriptionId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Gets prescription by id.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record GetIssuedPrescriptionByIdQuery(Guid PrescriptionId) : IQuery<IssuedPrescriptionDto>;

/// <summary>Lists prescriptions for an appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record ListIssuedPrescriptionsByAppointmentQuery(Guid AppointmentId) : IQuery<IReadOnlyList<IssuedPrescriptionDto>>;

/// <summary>Requests a clinical exam.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record RequestClinicalExamCommand(Guid AppointmentId, string Name, string Category, Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Completes an exam with optional result.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record CompleteClinicalExamCommand(Guid ExamId, string? ResultSummary, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Cancels a requested exam.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record CancelClinicalExamCommand(Guid ExamId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Lists exams for appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record ListClinicalExamsByAppointmentQuery(Guid AppointmentId) : IQuery<IReadOnlyList<ClinicalExamDto>>;

/// <summary>Lists exams for pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record ListClinicalExamsByPetQuery(Guid PetId) : IQuery<IReadOnlyList<ClinicalExamDto>>;

/// <summary>Uploads attachment bytes (online).</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record UploadClinicalAttachmentCommand(
    Guid AppointmentId,
    Guid? MedicalRecordId,
    Guid? ClinicalExamId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Soft-deletes attachment metadata.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public sealed record SoftDeleteClinicalAttachmentCommand(Guid AttachmentId, Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Gets attachment metadata.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record GetClinicalAttachmentQuery(Guid AttachmentId) : IQuery<ClinicalAttachmentDto>;

/// <summary>Opens blob stream for authorized download.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record DownloadClinicalAttachmentQuery(Guid AttachmentId) : IQuery<DownloadClinicalAttachmentResult>;

/// <summary>Lists attachments for appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public sealed record ListClinicalAttachmentsByAppointmentQuery(Guid AppointmentId) : IQuery<IReadOnlyList<ClinicalAttachmentDto>>;

/// <summary>Download query payload.</summary>
public sealed record DownloadClinicalAttachmentResult(string FileName, string ContentType, Stream Content);

/// <summary>Input line for template/prescription mutations.</summary>
public sealed record PrescriptionLineInput(
    Guid Id,
    string MedicationName,
    string Concentration,
    string Dose,
    string Route,
    string Frequency,
    string Duration,
    string Instructions,
    int SortOrder);
