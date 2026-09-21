using Core.Domain;
using Petshop.Domain.Enums;

namespace Petshop.Domain.Entities;

/// <summary>
/// Salon service catalog entry with default supply recipe (not Sales prepaid packages).
/// </summary>
public sealed class GroomingService : AggregateRoot
{
    private readonly List<GroomingServiceSupplyLine> _defaultSupplies = new();

    public string Name { get; private set; } = string.Empty;
    public GroomingServiceType ServiceType { get; private set; }
    public int DurationInMinutes { get; private set; }

    /// <summary>Optional link to Sales prepaid <c>ServiceCode</c> name (Banho/Tosa).</summary>
    public string? PrepaidServiceCode { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<GroomingServiceSupplyLine> DefaultSupplies => _defaultSupplies.AsReadOnly();

    private GroomingService() { }

    private GroomingService(Guid id, string name, GroomingServiceType serviceType, int durationInMinutes, string? prepaidServiceCode)
        : base(id)
    {
        Name = name;
        ServiceType = serviceType;
        DurationInMinutes = durationInMinutes;
        PrepaidServiceCode = prepaidServiceCode;
    }

    /// <summary>Creates a new active grooming service.</summary>
    public static Result<GroomingService> Create(
        Guid id,
        string name,
        GroomingServiceType serviceType,
        int durationInMinutes,
        string? prepaidServiceCode)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            return Result.Failure<GroomingService>(ErrorCodes.GroomingService.InvalidName);
        }

        if (durationInMinutes <= 0)
        {
            return Result.Failure<GroomingService>(ErrorCodes.GroomingService.InvalidDuration);
        }

        var service = new GroomingService(id, name.Trim(), serviceType, durationInMinutes, prepaidServiceCode?.Trim());
        service.Touch();
        return Result.Success(service);
    }

    /// <summary>Replaces default supply lines from catalog configuration.</summary>
    public Result SetDefaultSupplies(IEnumerable<(Guid ProductId, decimal Quantity)> lines)
    {
        _defaultSupplies.Clear();
        foreach (var (productId, quantity) in lines)
        {
            var line = GroomingServiceSupplyLine.Create(Id, productId, quantity);
            if (line.IsFailure)
            {
                return Result.Failure(line.Error);
            }

            _defaultSupplies.Add(line.Value);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Updates display metadata.</summary>
    public Result Update(string name, GroomingServiceType serviceType, int durationInMinutes, string? prepaidServiceCode, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            return Result.Failure(ErrorCodes.GroomingService.InvalidName);
        }

        if (durationInMinutes <= 0)
        {
            return Result.Failure(ErrorCodes.GroomingService.InvalidDuration);
        }

        Name = name.Trim();
        ServiceType = serviceType;
        DurationInMinutes = durationInMinutes;
        PrepaidServiceCode = prepaidServiceCode?.Trim();
        IsActive = isActive;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
