using Core.Domain;
using System.Text.RegularExpressions;

namespace Core.Domain.ValueObjects;

/// <summary>
/// Brazilian postal address used for fiscal recipient (NF-e enderDest).
/// </summary>
public sealed partial record PostalAddress
{
    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string District { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public int IbgeCityCode { get; }

    private PostalAddress(
        string street,
        string number,
        string? complement,
        string district,
        string city,
        string state,
        string postalCode,
        int ibgeCityCode)
    {
        Street = street;
        Number = number;
        Complement = complement;
        District = district;
        City = city;
        State = state;
        PostalCode = postalCode;
        IbgeCityCode = ibgeCityCode;
    }

    /// <summary>Creates a validated postal address.</summary>
    public static Result<PostalAddress> Create(
        string street,
        string number,
        string? complement,
        string district,
        string city,
        string state,
        string postalCode,
        int ibgeCityCode)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            return Result.Failure<PostalAddress>(new Error("Address.InvalidStreet", "Street is required."));
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            return Result.Failure<PostalAddress>(new Error("Address.InvalidNumber", "Number is required."));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return Result.Failure<PostalAddress>(new Error("Address.InvalidCity", "City is required."));
        }

        if (string.IsNullOrWhiteSpace(state) || state.Trim().Length != 2)
        {
            return Result.Failure<PostalAddress>(new Error("Address.InvalidState", "State must be a 2-letter UF."));
        }

        var cep = new string((postalCode ?? string.Empty).Where(char.IsDigit).ToArray());
        if (!CepDigits().IsMatch(cep))
        {
            return Result.Failure<PostalAddress>(new Error("Address.InvalidPostalCode", "CEP must have 8 digits."));
        }

        if (ibgeCityCode <= 0)
        {
            return Result.Failure<PostalAddress>(new Error("Address.InvalidIbge", "IBGE city code is required."));
        }

        return Result.Success(new PostalAddress(
            street.Trim(),
            number.Trim(),
            string.IsNullOrWhiteSpace(complement) ? null : complement.Trim(),
            district?.Trim() ?? string.Empty,
            city.Trim(),
            state.Trim().ToUpperInvariant(),
            cep,
            ibgeCityCode));
    }

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex CepDigits();
}
