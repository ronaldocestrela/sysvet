using System.Text.RegularExpressions;

namespace Core.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um endereço de e-mail válido.
/// </summary>
public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Endereço de e-mail normalizado em letras minúsculas.
    /// </summary>
    public string Address { get; }

    private Email(string address)
    {
        Address = address;
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="Email"/> após validação.
    /// </summary>
    /// <param name="address">Endereço de e-mail a ser validado.</param>
    /// <returns>Result contendo o Email ou erro de validação.</returns>
    public static Result<Email> Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result.Failure<Email>(new Error("Email.InvalidFormat", "O e-mail não pode ser vazio."));
        }

        var trimmed = address.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(trimmed))
        {
            return Result.Failure<Email>(new Error("Email.InvalidFormat", "O e-mail fornecido possui um formato inválido."));
        }

        return Result.Success(new Email(trimmed));
    }

    public override string ToString() => Address;
}
