using System.Text.RegularExpressions;

namespace Core.Domain.ValueObjects;

/// <summary>
/// Value Object que representa um CPF brasileiro válido.
/// </summary>
public record Cpf
{
    /// <summary>
    /// Número do CPF contendo apenas os 11 dígitos numéricos.
    /// </summary>
    public string Number { get; }

    private Cpf(string number)
    {
        Number = number;
    }

    /// <summary>
    /// Cria uma nova instância de <see cref="Cpf"/> após validação.
    /// </summary>
    /// <param name="rawCpf">String do CPF (com ou sem formatação).</param>
    /// <returns>Result contendo o Cpf ou erro de validação.</returns>
    public static Result<Cpf> Create(string rawCpf)
    {
        if (string.IsNullOrWhiteSpace(rawCpf))
        {
            return Result.Failure<Cpf>(new Error("Cpf.InvalidFormat", "O CPF não pode ser vazio."));
        }

        var cleaned = Regex.Replace(rawCpf, @"[^\d]", "");

        if (cleaned.Length != 11)
        {
            return Result.Failure<Cpf>(new Error("Cpf.InvalidFormat", "O CPF deve conter exatamente 11 dígitos."));
        }

        // Verifica se todos os dígitos são iguais (ex: 111.111.111-11 é inválido)
        if (new string(cleaned[0], 11) == cleaned)
        {
            return Result.Failure<Cpf>(new Error("Cpf.InvalidFormat", "CPF com dígitos repetidos é inválido."));
        }

        if (!IsValidChecksum(cleaned))
        {
            return Result.Failure<Cpf>(new Error("Cpf.InvalidFormat", "Dígito verificador de CPF inválido."));
        }

        return Result.Success(new Cpf(cleaned));
    }

    private static bool IsValidChecksum(string cpf)
    {
        var tempCpf = cpf.Substring(0, 9);
        var sum = 0;

        for (var i = 0; i < 9; i++)
        {
            sum += int.Parse(tempCpf[i].ToString()) * (10 - i);
        }

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        tempCpf += firstDigit;
        sum = 0;

        for (var i = 0; i < 10; i++)
        {
            sum += int.Parse(tempCpf[i].ToString()) * (11 - i);
        }

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return cpf.EndsWith($"{firstDigit}{secondDigit}");
    }

    public override string ToString() => Convert.ToUInt64(Number).ToString(@"000\.000\.000\-00");
}
