namespace Core.Domain;

/// <summary>
/// Representa o resultado de uma validação que falhou, contendo a lista de erros de validação.
/// </summary>
public sealed class ValidationResult : Result, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(false, new Error("Validation.Error", "A validation error occurred."))
    {
        ValidationErrors = errors;
    }

    /// <summary>
    /// Lista de erros de validação específicos.
    /// </summary>
    public Error[] ValidationErrors { get; }

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

/// <summary>
/// Representa o resultado de uma validação que falhou e que deveria retornar um valor em caso de sucesso.
/// </summary>
/// <typeparam name="TValue">Tipo do valor retornado pela operação em caso de sucesso.</typeparam>
public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    private ValidationResult(Error[] errors)
        : base(default, false, new Error("Validation.Error", "A validation error occurred."))
    {
        ValidationErrors = errors;
    }

    /// <summary>
    /// Lista de erros de validação específicos.
    /// </summary>
    public Error[] ValidationErrors { get; }

    public static ValidationResult<TValue> WithErrors(Error[] errors) => new(errors);
}
