using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Super Admin marketing/sales spend for CAC (roadmap 10.2).</summary>
public sealed class AcquisitionSpend : Entity
{
    /// <summary>Civil year (business timezone month bucket).</summary>
    public int Year { get; private set; }

    /// <summary>Civil month 1–12.</summary>
    public int Month { get; private set; }

    /// <summary>Acquisition channel label (e.g. ads, events).</summary>
    public string Channel { get; private set; } = string.Empty;

    /// <summary>Spend amount in BRL.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Optional note for operators.</summary>
    public string? Note { get; private set; }

#pragma warning disable CS8618
    private AcquisitionSpend()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a spend row for one month and channel.</summary>
    public static Result<AcquisitionSpend> Create(int year, int month, string channel, decimal amount, string? note)
    {
        if (year < 2000 || year > 9999)
        {
            return Result.Failure<AcquisitionSpend>(ErrorCodes.Metrics.InvalidYear);
        }

        if (month is < 1 or > 12)
        {
            return Result.Failure<AcquisitionSpend>(ErrorCodes.Metrics.InvalidMonth);
        }

        var normalizedChannel = channel?.Trim() ?? string.Empty;
        if (normalizedChannel.Length is < 1 or > 64)
        {
            return Result.Failure<AcquisitionSpend>(ErrorCodes.Metrics.InvalidChannel);
        }

        if (amount < 0)
        {
            return Result.Failure<AcquisitionSpend>(ErrorCodes.Metrics.InvalidAmount);
        }

        return Result.Success(new AcquisitionSpend
        {
            Id = Guid.NewGuid(),
            Year = year,
            Month = month,
            Channel = normalizedChannel,
            Amount = amount,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Replaces amount and note on upsert.</summary>
    public Result Update(decimal amount, string? note)
    {
        if (amount < 0)
        {
            return Result.Failure(ErrorCodes.Metrics.InvalidAmount);
        }

        Amount = amount;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
