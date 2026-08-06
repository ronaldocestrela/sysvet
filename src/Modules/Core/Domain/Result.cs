namespace Core.Domain;

/// <summary>
/// Representa um erro de domínio ou aplicação.
/// </summary>
/// <param name="Code">Código interno de erro para internacionalização ou tratamento programático (ex: "User.NotFound").</param>
/// <param name="Message">Mensagem legível sobre o erro.</param>
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
}

/// <summary>
/// Representa o resultado de uma operação, substituindo exceções para regras de negócio.
/// Contém o status de sucesso ou falha e o erro associado, se houver.
/// </summary>
public class Result
{
    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Um resultado de sucesso não pode conter um erro.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Um resultado de falha deve conter um erro.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Indica se a operação foi concluída com sucesso.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação resultou em falha.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Detalhes do erro, caso a operação tenha falhado.
    /// </summary>
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Representa o resultado de uma operação que retorna um valor em caso de sucesso.
/// </summary>
/// <typeparam name="TValue">Tipo do valor retornado pela operação.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// O valor do resultado. Dispara uma exceção se a operação tiver falhado e for acessado.
    /// </summary>
    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado que falhou.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}
