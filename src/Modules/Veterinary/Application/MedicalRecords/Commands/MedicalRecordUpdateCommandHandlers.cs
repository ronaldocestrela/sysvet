using Core.Domain;
using Core.Domain.Auditing;
using MediatR;
using Veterinary.Application.MedicalRecords;
using Veterinary.Domain.Repositories;
using Veterinary.Domain.ValueObjects;

namespace Veterinary.Application.MedicalRecords.Commands;

/// <summary>Handles anamnesis updates on medical records.</summary>
public sealed class UpdateAnamnesisCommandHandler : IRequestHandler<UpdateAnamnesisCommand, Result>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public UpdateAnamnesisCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateAnamnesisCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var update = record.SetAnamnesis(request.Anamnesis);
        if (update.IsFailure)
        {
            return update;
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(_auditLogger, _tenantContext, record.Id, "UpdateAnamnesis", "field=Anamnesis", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Handles vital signs capture on medical records.</summary>
public sealed class RecordVitalSignsCommandHandler : IRequestHandler<RecordVitalSignsCommand, Result>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public RecordVitalSignsCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(RecordVitalSignsCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var vitalsResult = VitalSigns.Create(
            request.WeightKg,
            request.TemperatureC,
            request.HeartRateBpm,
            request.RespiratoryRateBpm,
            request.MucousMembranes,
            request.CapillaryRefillTime,
            request.MeasuredAt);

        if (vitalsResult.IsFailure)
        {
            return Result.Failure(vitalsResult.Error);
        }

        var update = record.RecordVitalSigns(vitalsResult.Value);
        if (update.IsFailure)
        {
            return update;
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(_auditLogger, _tenantContext, record.Id, "RecordVitalSigns", "field=VitalSigns", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Handles evolution note append on medical records.</summary>
public sealed class AddEvolutionNoteCommandHandler : IRequestHandler<AddEvolutionNoteCommand, Result<Guid>>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public AddEvolutionNoteCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(AddEvolutionNoteCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var noteId = request.NoteId == Guid.Empty ? Guid.NewGuid() : request.NoteId;
        var recordedAt = request.RecordedAt ?? DateTimeOffset.UtcNow;
        var authorId = _tenantContext.UserId;

        var addResult = record.AddEvolutionNote(noteId, authorId, request.Text, recordedAt);
        if (addResult.IsFailure)
        {
            return addResult;
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(_auditLogger, _tenantContext, record.Id, "AddEvolutionNote", $"noteId={noteId:N}", cancellationToken);
        return Result.Success(noteId);
    }
}

/// <summary>Handles diagnosis updates on medical records.</summary>
public sealed class SetDiagnosisCommandHandler : IRequestHandler<SetDiagnosisCommand, Result>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public SetDiagnosisCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SetDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var update = record.SetDiagnosis(request.Diagnosis);
        if (update.IsFailure)
        {
            return Result.Failure(update.Error);
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(_auditLogger, _tenantContext, record.Id, "SetDiagnosis", "field=Diagnosis", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Handles conduct/plan updates on medical records.</summary>
public sealed class SetConductCommandHandler : IRequestHandler<SetConductCommand, Result>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public SetConductCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SetConductCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var update = record.SetConduct(request.Conduct);
        if (update.IsFailure)
        {
            return Result.Failure(update.Error);
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(_auditLogger, _tenantContext, record.Id, "SetConduct", "field=Conduct", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Handles medical record finalization.</summary>
public sealed class FinalizeMedicalRecordCommandHandler : IRequestHandler<FinalizeMedicalRecordCommand, Result>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public FinalizeMedicalRecordCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(FinalizeMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var finalize = record.FinalizeRecord();
        if (finalize.IsFailure)
        {
            return Result.Failure(finalize.Error);
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(_auditLogger, _tenantContext, record.Id, "Finalize", "status=Finalized", cancellationToken);
        return Result.Success();
    }
}

/// <summary>Handles follow-up date updates on draft medical records.</summary>
public sealed class SetFollowUpOnCommandHandler : IRequestHandler<SetFollowUpOnCommand, Result>
{
    private readonly IMedicalRecordRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public SetFollowUpOnCommandHandler(IMedicalRecordRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SetFollowUpOnCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.MedicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Veterinary.Domain.ErrorCodes.MedicalRecord.NotFound);
        }

        var update = record.SetFollowUpOn(request.FollowUpOn);
        if (update.IsFailure)
        {
            return Result.Failure(update.Error);
        }

        _repository.Update(record);
        await MedicalRecordAuditHelper.LogAsync(
            _auditLogger,
            _tenantContext,
            record.Id,
            "SetFollowUpOn",
            $"followUpOn={request.FollowUpOn}",
            cancellationToken);
        return Result.Success();
    }
}
