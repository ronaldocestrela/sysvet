using System.Security.Cryptography;
using System.Text;
using Core.Domain.ValueObjects;

namespace Core.Domain.Privacy;

/// <summary>
/// Deterministic tombstone values for tutor anonymization (unique per tutor id).
/// </summary>
public static class PersonalDataTombstone
{
    /// <summary>
    /// Builds display name for an anonymized tutor.
    /// </summary>
    public static string Name(Guid tutorId) => $"anon-{tutorId:N}";

    /// <summary>
    /// Builds a unique valid e-mail tombstone for an anonymized tutor.
    /// </summary>
    public static Result<Email> Email(Guid tutorId) =>
        ValueObjects.Email.Create($"anon-{tutorId:N}@anonymized.invalid");

    /// <summary>
    /// Builds a unique valid CPF tombstone derived from the tutor id (checksum-valid).
    /// </summary>
    public static Result<Cpf> Cpf(Guid tutorId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(tutorId.ToString("N")));
        var baseNine = new char[9];
        for (var i = 0; i < 9; i++)
        {
            baseNine[i] = (char)('0' + (hash[i] % 10));
        }

        if (new string(baseNine[0], 9) == new string(baseNine))
        {
            baseNine[8] = (char)('0' + ((hash[8] + 1) % 10));
        }

        var withFirst = new string(baseNine) + ComputeCpfCheckDigit(new string(baseNine), 10);
        var full = withFirst + ComputeCpfCheckDigit(withFirst, 11);
        return ValueObjects.Cpf.Create(full);
    }

    /// <summary>
    /// Builds a unique valid phone tombstone (11 digits with DDD).
    /// </summary>
    public static Result<Phone> Phone(Guid tutorId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes("phone:" + tutorId.ToString("N")));
        var digits = new StringBuilder(11);
        digits.Append("00");
        for (var i = 0; i < 9; i++)
        {
            digits.Append((char)('0' + (hash[i] % 10)));
        }

        return ValueObjects.Phone.Create(digits.ToString());
    }

    private static char ComputeCpfCheckDigit(string partial, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < partial.Length; i++)
        {
            sum += (partial[i] - '0') * (weightStart - i);
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;
        return (char)('0' + digit);
    }
}
