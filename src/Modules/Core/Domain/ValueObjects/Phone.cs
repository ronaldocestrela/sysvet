using System.Text.RegularExpressions;

namespace Core.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um número de telefone brasileiro válido.
/// </summary>
public record Phone
{
    /// <summary>
    /// Número de telefone contendo apenas dígitos numéricos (com DDD).
    /// </summary>
    public string Number { get; }

    private Phone(string number)
    {
        Number = number;
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="Phone"/> após validação.
    /// </summary>
    /// <param name="rawPhone">Número de telefone a ser validado.</param>
    /// <returns>Result contendo o Phone ou erro de validação.</returns>
    public static Result<Phone> Create(string rawPhone)
    {
        if (string.IsNullOrWhiteSpace(rawPhone))
        {
            return Result.Failure<Phone>(new Error("Phone.InvalidFormat", "O telefone não pode ser vazio."));
        }

        var cleaned = Regex.Replace(rawPhone, @"[^\d]", "");

        // Se começar com 55 (DDI Brasil), remove para padronizar com DDD + Número
        if (cleaned.Length is 12 or 13 && cleaned.StartsWith("55"))
        {
            cleaned = cleaned.Substring(2);
        }

        // Telefone fixo (10 dígitos) ou celular (11 dígitos)
        if (cleaned.Length is < 10 or > 11)
        {
            return Result.Failure<Phone>(new Error("Phone.InvalidFormat", "O telefone deve conter 10 ou 11 dígitos com DDD."));
        }

        return Result.Success(new Phone(cleaned));
    }

    public override string ToString() => Number;
}
