using Core.Domain;

namespace Petshop.Domain.Entities;

/// <summary>Lifecycle state of a grooming digital record.</summary>
public enum GroomingRecordStatus
{
    Draft = 1,
    Finalized = 2
}

/// <summary>
/// Digital bath and grooming record linked 1:1 to a grooming appointment.
/// </summary>
public sealed class GroomingRecord : AggregateRoot
{
    private readonly List<GroomingRecordSupplyLine> _supplyLines = new();

    public Guid GroomingAppointmentId { get; private set; }
    public Guid GroomerId { get; private set; }
    public Guid TutorId { get; private set; }
    public Guid PetId { get; private set; }
    public string CoatNotes { get; private set; } = string.Empty;
    public GroomingRecordStatus Status { get; private set; }

    public IReadOnlyCollection<GroomingRecordSupplyLine> SupplyLines => _supplyLines.AsReadOnly();

    private GroomingRecord() { }

    private GroomingRecord(Guid id, Guid groomingAppointmentId, Guid groomerId, Guid tutorId, Guid petId)
        : base(id)
    {
        GroomingAppointmentId = groomingAppointmentId;
        GroomerId = groomerId;
        TutorId = tutorId;
        PetId = petId;
        Status = GroomingRecordStatus.Draft;
    }

    /// <summary>Creates a draft record for a new appointment.</summary>
    public static Result<GroomingRecord> Create(
        Guid id,
        Guid groomingAppointmentId,
        Guid groomerId,
        Guid tutorId,
        Guid petId)
    {
        if (groomingAppointmentId == Guid.Empty || groomerId == Guid.Empty || tutorId == Guid.Empty || petId == Guid.Empty)
        {
            return Result.Failure<GroomingRecord>(ErrorCodes.GroomingRecord.InvalidIdentifiers);
        }

        var record = new GroomingRecord(id, groomingAppointmentId, groomerId, tutorId, petId);
        record.Touch();
        return Result.Success(record);
    }

    /// <summary>Seeds supply lines from the service catalog recipe.</summary>
    public Result SeedSuppliesFromService(GroomingService service)
    {
        if (Status == GroomingRecordStatus.Finalized)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.Finalized);
        }

        _supplyLines.Clear();
        foreach (var line in service.DefaultSupplies)
        {
            var created = GroomingRecordSupplyLine.Create(Id, line.ProductId, line.Quantity);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            _supplyLines.Add(created.Value);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Updates coat notes on a draft record.</summary>
    public Result UpdateCoatNotes(string coatNotes)
    {
        if (Status == GroomingRecordStatus.Finalized)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.Finalized);
        }

        if (coatNotes.Length > 2000)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.InvalidCoatNotes);
        }

        CoatNotes = coatNotes ?? string.Empty;
        Touch();
        return Result.Success();
    }

    /// <summary>Replaces editable supply lines on a draft record.</summary>
    public Result SetSupplyLines(IEnumerable<(Guid ProductId, decimal Quantity)> lines)
    {
        if (Status == GroomingRecordStatus.Finalized)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.Finalized);
        }

        _supplyLines.Clear();
        foreach (var (productId, quantity) in lines)
        {
            var line = GroomingRecordSupplyLine.Create(Id, productId, quantity);
            if (line.IsFailure)
            {
                return Result.Failure(line.Error);
            }

            _supplyLines.Add(line.Value);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Locks the record when the appointment is completed.</summary>
    public Result FinalizeRecord()
    {
        if (Status == GroomingRecordStatus.Finalized)
        {
            return Result.Failure(ErrorCodes.GroomingRecord.AlreadyFinalized);
        }

        Status = GroomingRecordStatus.Finalized;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
