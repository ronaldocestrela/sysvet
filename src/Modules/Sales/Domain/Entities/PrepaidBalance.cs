using Core.Domain;
using Sales.Domain.Enums;

namespace Sales.Domain.Entities;

/// <summary>Prepaid service uses wallet for a pet and service type.</summary>
public sealed class PrepaidBalance : AggregateRoot
{
    public Guid TutorId { get; private set; }
    public Guid PetId { get; private set; }
    public ServiceCode ServiceCode { get; private set; }
    public int RemainingUses { get; private set; }
    public int PurchasedUses { get; private set; }

    private readonly List<PrepaidCredit> _credits = new();
    public IReadOnlyCollection<PrepaidCredit> Credits => _credits.AsReadOnly();

    private PrepaidBalance() { }

    private PrepaidBalance(Guid id, Guid tutorId, Guid petId, ServiceCode serviceCode)
        : base(id)
    {
        TutorId = tutorId;
        PetId = petId;
        ServiceCode = serviceCode;
    }

    /// <summary>Opens a new balance row for pet + service.</summary>
    public static Result<PrepaidBalance> Create(Guid tutorId, Guid petId, ServiceCode serviceCode)
        => Create(Guid.NewGuid(), tutorId, petId, serviceCode);

    /// <summary>Opens a balance with a known id (sync).</summary>
    public static Result<PrepaidBalance> Create(Guid id, Guid tutorId, Guid petId, ServiceCode serviceCode)
    {
        if (id == Guid.Empty || tutorId == Guid.Empty || petId == Guid.Empty)
        {
            return Result.Failure<PrepaidBalance>(ErrorCodes.Package.PetRequired);
        }

        return Result.Success(new PrepaidBalance(id, tutorId, petId, serviceCode));
    }

    /// <summary>Credits uses from a paid package line (idempotent per order item).</summary>
    public Result Credit(Guid orderId, Guid orderItemId, int uses)
    {
        if (uses <= 0)
        {
            return Result.Failure(ErrorCodes.Package.InvalidUses);
        }

        if (_credits.Any(c => c.OrderItemId == orderItemId))
        {
            return Result.Success();
        }

        _credits.Add(new PrepaidCredit(Guid.NewGuid(), Id, orderId, orderItemId, uses));
        RemainingUses += uses;
        PurchasedUses += uses;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Reverses credit from a returned package line.</summary>
    public Result ReverseCredit(Guid orderItemId, int usesToReverse)
    {
        if (usesToReverse > RemainingUses)
        {
            return Result.Failure(ErrorCodes.Package.ReturnAfterConsumption);
        }

        var credit = _credits.FirstOrDefault(c => c.OrderItemId == orderItemId);
        if (credit is null)
        {
            return Result.Success();
        }

        var reverse = credit.Reverse(usesToReverse);
        if (reverse.IsFailure)
        {
            return reverse;
        }

        RemainingUses -= usesToReverse;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Consumes one prepaid use (caller ensures idempotency via UsageId at application layer).</summary>
    public Result Consume(ServiceCode serviceCode, Guid petId)
    {
        if (petId != PetId)
        {
            return Result.Failure(ErrorCodes.Package.PetMismatch);
        }

        if (serviceCode != ServiceCode)
        {
            return Result.Failure(ErrorCodes.Package.ServiceMismatch);
        }

        if (RemainingUses < 1)
        {
            return Result.Failure(ErrorCodes.Package.InsufficientBalance);
        }

        RemainingUses--;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
