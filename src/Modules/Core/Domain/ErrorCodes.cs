namespace Core.Domain;

/// <summary>
/// Códigos de erro padronizados para o módulo Core.
/// </summary>
public static class ErrorCodes
{
    public static class Tutor
    {
        public static readonly Error NotFound = new("Tutor.NotFound", "O tutor especificado não foi encontrado.");
        public static readonly Error InvalidCpf = new("Tutor.InvalidCpf", "O CPF fornecido é inválido ou já está em uso.");
    }

    public static class Pet
    {
        public static readonly Error NotFound = new("Pet.NotFound", "O pet especificado não foi encontrado.");
    }
}
