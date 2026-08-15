using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>
/// Aggregate Root que representa o Tutor do pet.
/// </summary>
public class Tutor : AggregateRoot
{
    private readonly List<Pet> _pets = new();

    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Cpf Cpf { get; private set; }
    public Phone Phone { get; private set; }

    /// <summary>
    /// Lista imutável de pets vinculados ao tutor.
    /// </summary>
    public IReadOnlyCollection<Pet> Pets => _pets.AsReadOnly();

    private Tutor(Guid id, string name, Email email, Cpf cpf, Phone phone)
        : base(id)
    {
        Name = name;
        Email = email;
        Cpf = cpf;
        Phone = phone;
    }

    /// <summary>
    /// Factory Method para criação de um Tutor com validação.
    /// </summary>
    public static Result<Tutor> Create(string name, Email email, Cpf cpf, Phone phone, Guid id = default)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
        {
            return Result.Failure<Tutor>(new Error("Tutor.InvalidName", "O nome do tutor deve ter pelo menos 2 caracteres."));
        }

        if (email is null)
        {
            return Result.Failure<Tutor>(new Error("Tutor.NullEmail", "O e-mail é obrigatório."));
        }

        if (cpf is null)
        {
            return Result.Failure<Tutor>(new Error("Tutor.NullCpf", "O CPF é obrigatório."));
        }

        if (phone is null)
        {
            return Result.Failure<Tutor>(new Error("Tutor.NullPhone", "O telefone é obrigatório."));
        }

        var tutor = new Tutor(id, name.Trim(), email, cpf, phone);
        return Result.Success(tutor);
    }

    /// <summary>
    /// Adiciona um pet à lista do tutor.
    /// </summary>
    public Result AddPet(Pet pet)
    {
        if (pet is null)
        {
            return Result.Failure(new Error("Tutor.NullPet", "Não é possível adicionar um pet nulo."));
        }

        _pets.Add(pet);
        return Result.Success();
    }
}
