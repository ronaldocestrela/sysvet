namespace Core.Domain;

public interface IValidationResult
{
    Error[] ValidationErrors { get; }
}
