using Core.Application.Storage;
using Core.Domain;
using Core.Domain.Auditing;
using MediatR;
using Veterinary.Application.Clinical.Dtos;
using Veterinary.Domain.Entities;
using VetErrors = Veterinary.Domain.ErrorCodes;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;
using Veterinary.Domain.ValueObjects;

namespace Veterinary.Application.Clinical.Commands;

/// <summary>Handles prescription template and issued prescription commands.</summary>
public sealed class CreatePrescriptionTemplateCommandHandler : IRequestHandler<CreatePrescriptionTemplateCommand, Result<Guid>>
{
    private readonly IPrescriptionTemplateRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public CreatePrescriptionTemplateCommandHandler(IPrescriptionTemplateRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreatePrescriptionTemplateCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var created = PrescriptionTemplate.Create(id, request.Name, request.Species);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        var replace = created.Value.ReplaceItems(MapLines(id, request.Items));
        if (replace.IsFailure)
        {
            return Result.Failure<Guid>(replace.Error);
        }

        await _repository.AddAsync(created.Value, cancellationToken);
        await ClinicalAuditHelper.LogAsync(_auditLogger, _tenantContext, id, "PrescriptionTemplate", "Create", "status=Active", cancellationToken);
        return Result.Success(id);
    }

    private static IEnumerable<(Guid, string, string, string, string, string, string, string, int)> MapLines(Guid templateId, IReadOnlyList<PrescriptionLineInput> items) =>
        items.Select(i => (i.Id == Guid.Empty ? Guid.NewGuid() : i.Id, i.MedicationName, i.Concentration, i.Dose, i.Route, i.Frequency, i.Duration, i.Instructions, i.SortOrder));
}

public sealed class UpdatePrescriptionTemplateCommandHandler : IRequestHandler<UpdatePrescriptionTemplateCommand, Result>
{
    private readonly IPrescriptionTemplateRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public UpdatePrescriptionTemplateCommandHandler(IPrescriptionTemplateRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdatePrescriptionTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result.Failure(VetErrors.PrescriptionTemplate.NotFound);
        }

        var update = template.UpdateDetails(request.Name, request.Species);
        if (update.IsFailure)
        {
            return update;
        }

        var replace = template.ReplaceItems(request.Items.Select(i => (i.Id == Guid.Empty ? Guid.NewGuid() : i.Id, i.MedicationName, i.Concentration, i.Dose, i.Route, i.Frequency, i.Duration, i.Instructions, i.SortOrder)));
        if (replace.IsFailure)
        {
            return replace;
        }

        _repository.Update(template);
        await ClinicalAuditHelper.LogAsync(_auditLogger, _tenantContext, template.Id, "PrescriptionTemplate", "Update", "items replaced", cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivatePrescriptionTemplateCommandHandler : IRequestHandler<DeactivatePrescriptionTemplateCommand, Result>
{
    private readonly IPrescriptionTemplateRepository _repository;

    public DeactivatePrescriptionTemplateCommandHandler(IPrescriptionTemplateRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeactivatePrescriptionTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result.Failure(VetErrors.PrescriptionTemplate.NotFound);
        }

        var result = template.Deactivate();
        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(template);
        return Result.Success();
    }
}

public sealed class ListPrescriptionTemplatesQueryHandler : IRequestHandler<ListPrescriptionTemplatesQuery, Result<IReadOnlyList<PrescriptionTemplateDto>>>
{
    private readonly IPrescriptionTemplateRepository _repository;

    public ListPrescriptionTemplatesQueryHandler(IPrescriptionTemplateRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<PrescriptionTemplateDto>>> Handle(ListPrescriptionTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repository.ListActiveAsync(cancellationToken);
        return Result.Success<IReadOnlyList<PrescriptionTemplateDto>>(templates.Select(ClinicalMappings.MapTemplate).ToList());
    }
}

public sealed class CreateIssuedPrescriptionCommandHandler : IRequestHandler<CreateIssuedPrescriptionCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPrescriptionTemplateRepository _templateRepository;
    private readonly IIssuedPrescriptionRepository _prescriptionRepository;

    public CreateIssuedPrescriptionCommandHandler(
        IAppointmentRepository appointmentRepository,
        IPrescriptionTemplateRepository templateRepository,
        IIssuedPrescriptionRepository prescriptionRepository)
    {
        _appointmentRepository = appointmentRepository;
        _templateRepository = templateRepository;
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<Result<Guid>> Handle(CreateIssuedPrescriptionCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(VetErrors.Appointment.NotFound);
        }

        if (!appointment.IsEligibleForMedicalRecord())
        {
            return Result.Failure<Guid>(VetErrors.ClinicalExam.AppointmentNotEligible);
        }

        var id = Guid.NewGuid();
        var created = IssuedPrescription.Create(id, appointment.Id, appointment.PetId, appointment.VeterinarianId, request.TemplateId);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        if (request.TemplateId is Guid templateId)
        {
            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            if (template is null)
            {
                return Result.Failure<Guid>(VetErrors.PrescriptionTemplate.NotFound);
            }

            var copy = created.Value.CopyItemsFromTemplate(template);
            if (copy.IsFailure)
            {
                return Result.Failure<Guid>(copy.Error);
            }
        }

        await _prescriptionRepository.AddAsync(created.Value, cancellationToken);
        return Result.Success(id);
    }
}

public sealed class ReplaceIssuedPrescriptionItemsCommandHandler : IRequestHandler<ReplaceIssuedPrescriptionItemsCommand, Result>
{
    private readonly IIssuedPrescriptionRepository _repository;

    public ReplaceIssuedPrescriptionItemsCommandHandler(IIssuedPrescriptionRepository repository) => _repository = repository;

    public async Task<Result> Handle(ReplaceIssuedPrescriptionItemsCommand request, CancellationToken cancellationToken)
    {
        return await _repository.ReplaceDraftItemsAsync(
            request.PrescriptionId,
            request.Items.Select(i => (i.Id == Guid.Empty ? Guid.NewGuid() : i.Id, i.MedicationName, i.Concentration, i.Dose, i.Route, i.Frequency, i.Duration, i.Instructions, i.SortOrder)),
            cancellationToken);
    }
}

public sealed class IssuePrescriptionCommandHandler : IRequestHandler<IssuePrescriptionCommand, Result>
{
    private readonly IIssuedPrescriptionRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public IssuePrescriptionCommandHandler(IIssuedPrescriptionRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(IssuePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var prescription = await _repository.GetByIdAsync(request.PrescriptionId, cancellationToken);
        if (prescription is null)
        {
            return Result.Failure(VetErrors.IssuedPrescription.NotFound);
        }

        var issue = prescription.Issue();
        if (issue.IsFailure)
        {
            return issue;
        }

        await ClinicalAuditHelper.LogAsync(_auditLogger, _tenantContext, prescription.Id, "IssuedPrescription", "Issue", "status=Issued", cancellationToken);
        return Result.Success();
    }
}

public sealed class GetIssuedPrescriptionByIdQueryHandler : IRequestHandler<GetIssuedPrescriptionByIdQuery, Result<IssuedPrescriptionDto>>
{
    private readonly IIssuedPrescriptionRepository _repository;

    public GetIssuedPrescriptionByIdQueryHandler(IIssuedPrescriptionRepository repository) => _repository = repository;

    public async Task<Result<IssuedPrescriptionDto>> Handle(GetIssuedPrescriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var prescription = await _repository.GetByIdAsync(request.PrescriptionId, cancellationToken);
        return prescription is null
            ? Result.Failure<IssuedPrescriptionDto>(VetErrors.IssuedPrescription.NotFound)
            : Result.Success(ClinicalMappings.MapPrescription(prescription));
    }
}

public sealed class ListIssuedPrescriptionsByAppointmentQueryHandler : IRequestHandler<ListIssuedPrescriptionsByAppointmentQuery, Result<IReadOnlyList<IssuedPrescriptionDto>>>
{
    private readonly IIssuedPrescriptionRepository _repository;

    public ListIssuedPrescriptionsByAppointmentQueryHandler(IIssuedPrescriptionRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<IssuedPrescriptionDto>>> Handle(ListIssuedPrescriptionsByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        return Result.Success<IReadOnlyList<IssuedPrescriptionDto>>(list.Select(ClinicalMappings.MapPrescription).ToList());
    }
}

public sealed class RequestClinicalExamCommandHandler : IRequestHandler<RequestClinicalExamCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClinicalExamRepository _examRepository;

    public RequestClinicalExamCommandHandler(IAppointmentRepository appointmentRepository, IClinicalExamRepository examRepository)
    {
        _appointmentRepository = appointmentRepository;
        _examRepository = examRepository;
    }

    public async Task<Result<Guid>> Handle(RequestClinicalExamCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(VetErrors.Appointment.NotFound);
        }

        if (!appointment.IsEligibleForMedicalRecord())
        {
            return Result.Failure<Guid>(VetErrors.ClinicalExam.AppointmentNotEligible);
        }

        if (!Enum.TryParse<ClinicalExamCategory>(request.Category, true, out var category))
        {
            return Result.Failure<Guid>(VetErrors.ClinicalExam.InvalidName);
        }

        var id = Guid.NewGuid();
        var created = ClinicalExam.Request(id, appointment.Id, appointment.PetId, request.Name, category);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        await _examRepository.AddAsync(created.Value, cancellationToken);
        return Result.Success(id);
    }
}

public sealed class CompleteClinicalExamCommandHandler : IRequestHandler<CompleteClinicalExamCommand, Result>
{
    private readonly IClinicalExamRepository _repository;

    public CompleteClinicalExamCommandHandler(IClinicalExamRepository repository) => _repository = repository;

    public async Task<Result> Handle(CompleteClinicalExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _repository.GetByIdAsync(request.ExamId, cancellationToken);
        if (exam is null)
        {
            return Result.Failure(VetErrors.ClinicalExam.NotFound);
        }

        var complete = exam.Complete(request.ResultSummary);
        if (complete.IsFailure)
        {
            return complete;
        }

        _repository.Update(exam);
        return Result.Success();
    }
}

public sealed class CancelClinicalExamCommandHandler : IRequestHandler<CancelClinicalExamCommand, Result>
{
    private readonly IClinicalExamRepository _repository;

    public CancelClinicalExamCommandHandler(IClinicalExamRepository repository) => _repository = repository;

    public async Task<Result> Handle(CancelClinicalExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _repository.GetByIdAsync(request.ExamId, cancellationToken);
        if (exam is null)
        {
            return Result.Failure(VetErrors.ClinicalExam.NotFound);
        }

        var cancel = exam.Cancel();
        if (cancel.IsFailure)
        {
            return cancel;
        }

        _repository.Update(exam);
        return Result.Success();
    }
}

public sealed class ListClinicalExamsByAppointmentQueryHandler : IRequestHandler<ListClinicalExamsByAppointmentQuery, Result<IReadOnlyList<ClinicalExamDto>>>
{
    private readonly IClinicalExamRepository _repository;

    public ListClinicalExamsByAppointmentQueryHandler(IClinicalExamRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ClinicalExamDto>>> Handle(ListClinicalExamsByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var exams = await _repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        return Result.Success<IReadOnlyList<ClinicalExamDto>>(exams.Select(ClinicalMappings.MapExam).ToList());
    }
}

public sealed class ListClinicalExamsByPetQueryHandler : IRequestHandler<ListClinicalExamsByPetQuery, Result<IReadOnlyList<ClinicalExamDto>>>
{
    private readonly IClinicalExamRepository _repository;

    public ListClinicalExamsByPetQueryHandler(IClinicalExamRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ClinicalExamDto>>> Handle(ListClinicalExamsByPetQuery request, CancellationToken cancellationToken)
    {
        var exams = await _repository.GetByPetIdAsync(request.PetId, cancellationToken);
        return Result.Success<IReadOnlyList<ClinicalExamDto>>(exams.Select(ClinicalMappings.MapExam).ToList());
    }
}

public sealed class UploadClinicalAttachmentCommandHandler : IRequestHandler<UploadClinicalAttachmentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClinicalAttachmentRepository _attachmentRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public UploadClinicalAttachmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IClinicalAttachmentRepository attachmentRepository,
        IBlobStorage blobStorage,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _appointmentRepository = appointmentRepository;
        _attachmentRepository = attachmentRepository;
        _blobStorage = blobStorage;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(UploadClinicalAttachmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(VetErrors.Appointment.NotFound);
        }

        if (!appointment.IsEligibleForMedicalRecord())
        {
            return Result.Failure<Guid>(VetErrors.ClinicalAttachment.AppointmentNotEligible);
        }

        var spec = AttachmentFileSpec.Create(request.FileName, request.ContentType, request.SizeBytes);
        if (spec.IsFailure)
        {
            return Result.Failure<Guid>(spec.Error);
        }

        var id = Guid.NewGuid();
        var blobKey = ClinicalBlobKeyBuilder.Build(_tenantContext, id);
        var put = await _blobStorage.PutAsync(blobKey, request.Content, spec.Value.ContentType, cancellationToken);
        if (put.IsFailure)
        {
            return Result.Failure<Guid>(put.Error);
        }

        var created = ClinicalAttachment.Create(id, request.AppointmentId, request.MedicalRecordId, request.ClinicalExamId, spec.Value, blobKey);
        if (created.IsFailure)
        {
            await _blobStorage.DeleteAsync(blobKey, cancellationToken);
            return Result.Failure<Guid>(created.Error);
        }

        await _attachmentRepository.AddAsync(created.Value, cancellationToken);
        await ClinicalAuditHelper.LogAsync(_auditLogger, _tenantContext, id, "ClinicalAttachment", "Upload", $"kind={spec.Value.Kind}", cancellationToken);
        return Result.Success(id);
    }
}

public sealed class SoftDeleteClinicalAttachmentCommandHandler : IRequestHandler<SoftDeleteClinicalAttachmentCommand, Result>
{
    private readonly IClinicalAttachmentRepository _repository;

    public SoftDeleteClinicalAttachmentCommandHandler(IClinicalAttachmentRepository repository) => _repository = repository;

    public async Task<Result> Handle(SoftDeleteClinicalAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _repository.GetByIdAsync(request.AttachmentId, cancellationToken);
        if (attachment is null)
        {
            return Result.Failure(VetErrors.ClinicalAttachment.NotFound);
        }

        var delete = attachment.SoftDelete();
        if (delete.IsFailure)
        {
            return delete;
        }

        _repository.Update(attachment);
        return Result.Success();
    }
}

public sealed class GetClinicalAttachmentQueryHandler : IRequestHandler<GetClinicalAttachmentQuery, Result<ClinicalAttachmentDto>>
{
    private readonly IClinicalAttachmentRepository _repository;

    public GetClinicalAttachmentQueryHandler(IClinicalAttachmentRepository repository) => _repository = repository;

    public async Task<Result<ClinicalAttachmentDto>> Handle(GetClinicalAttachmentQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _repository.GetByIdAsync(request.AttachmentId, cancellationToken);
        if (attachment is null || attachment.IsDeleted)
        {
            return Result.Failure<ClinicalAttachmentDto>(VetErrors.ClinicalAttachment.NotFound);
        }

        return Result.Success(ClinicalMappings.MapAttachment(attachment));
    }
}

public sealed class DownloadClinicalAttachmentQueryHandler : IRequestHandler<DownloadClinicalAttachmentQuery, Result<DownloadClinicalAttachmentResult>>
{
    private readonly IClinicalAttachmentRepository _repository;
    private readonly IBlobStorage _blobStorage;

    public DownloadClinicalAttachmentQueryHandler(IClinicalAttachmentRepository repository, IBlobStorage blobStorage)
    {
        _repository = repository;
        _blobStorage = blobStorage;
    }

    public async Task<Result<DownloadClinicalAttachmentResult>> Handle(DownloadClinicalAttachmentQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _repository.GetByIdAsync(request.AttachmentId, cancellationToken);
        if (attachment is null || attachment.IsDeleted)
        {
            return Result.Failure<DownloadClinicalAttachmentResult>(VetErrors.ClinicalAttachment.NotFound);
        }

        var stream = await _blobStorage.OpenReadAsync(attachment.BlobKey, cancellationToken);
        if (stream.IsFailure)
        {
            return Result.Failure<DownloadClinicalAttachmentResult>(stream.Error);
        }

        return Result.Success(new DownloadClinicalAttachmentResult(attachment.FileName, attachment.ContentType, stream.Value));
    }
}

public sealed class ListClinicalAttachmentsByAppointmentQueryHandler : IRequestHandler<ListClinicalAttachmentsByAppointmentQuery, Result<IReadOnlyList<ClinicalAttachmentDto>>>
{
    private readonly IClinicalAttachmentRepository _repository;

    public ListClinicalAttachmentsByAppointmentQueryHandler(IClinicalAttachmentRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<ClinicalAttachmentDto>>> Handle(ListClinicalAttachmentsByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var attachments = await _repository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
        return Result.Success<IReadOnlyList<ClinicalAttachmentDto>>(attachments.Select(ClinicalMappings.MapAttachment).ToList());
    }
}
