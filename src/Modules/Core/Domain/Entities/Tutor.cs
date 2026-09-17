using Core.Domain.Events;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>
/// Aggregate Root que representa o Tutor do pet.
/// </summary>
public class Tutor : AggregateRoot, ISoftDeletable, IAuditable
{
    private readonly List<Pet> _pets = new();

    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Cpf Cpf { get; private set; }
    public Phone Phone { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>
    /// Tutores ativos podem receber novos pets e ser referenciados em atendimentos.
    /// </summary>
    public bool IsActive => !IsDeleted;

    /// <summary>
    /// Lista imutável de pets vinculados ao tutor.
    /// </summary>
    public IReadOnlyCollection<Pet> Pets => _pets.AsReadOnly();

#pragma warning disable CS8618
    protected Tutor() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

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
            return Result.Failure<Tutor>(ErrorCodes.Tutor.InvalidName);
        }

        if (email is null)
        {
            return Result.Failure<Tutor>(ErrorCodes.Tutor.NullEmail);
        }

        if (cpf is null)
        {
            return Result.Failure<Tutor>(ErrorCodes.Tutor.NullCpf);
        }

        if (phone is null)
        {
            return Result.Failure<Tutor>(ErrorCodes.Tutor.NullPhone);
        }

        var tutor = new Tutor(id, name.Trim(), email, cpf, phone);
        tutor.Raise(new TutorRegisteredDomainEvent(tutor.Id, DateTimeOffset.UtcNow));
        return Result.Success(tutor);
    }

    /// <summary>
    /// Atualiza dados mutáveis do tutor (CPF permanece imutável após o cadastro).
    /// </summary>
    public Result Update(string name, Email email, Phone phone)
    {
        if (IsDeleted)
        {
            return Result.Failure(ErrorCodes.Tutor.AlreadyDeleted);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
        {
            return Result.Failure(ErrorCodes.Tutor.InvalidName);
        }

        if (email is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NullEmail);
        }

        if (phone is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NullPhone);
        }

        Name = name.Trim();
        Email = email;
        Phone = phone;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Marca o tutor como excluído logicamente; operação idempotente.
    /// </summary>
    public Result SoftDelete()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        return Result.Success();
    }

    /// <summary>
    /// Adiciona um pet à lista do tutor enquanto o tutor estiver ativo.
    /// </summary>
    public Result AddPet(Pet pet)
    {
        if (pet is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NullPet);
        }

        if (!IsActive)
        {
            return Result.Failure(ErrorCodes.Pet.TutorInactive);
        }

        _pets.Add(pet);
        return Result.Success();
    }
}
