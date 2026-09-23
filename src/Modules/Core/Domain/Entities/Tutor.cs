using Core.Domain.Events;
using Core.Domain.Privacy;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>
/// Aggregate Root que representa o Tutor do pet.
/// </summary>
public class Tutor : AggregateRoot, ISoftDeletable, IAnonymizable, IAuditable
{
    private readonly List<Pet> _pets = new();

    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Cpf Cpf { get; private set; }
    public Phone Phone { get; private set; }

    /// <summary>Optional address for NF-e recipient (enderDest).</summary>
    public PostalAddress? Address { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <inheritdoc />
    public bool IsAnonymized { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? AnonymizedAt { get; private set; }

    /// <summary>
    /// Tutores ativos podem receber novos pets e ser referenciados em atendimentos.
    /// </summary>
    public bool IsActive => !IsDeleted && !IsAnonymized;

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
    /// <summary>Sets or clears the tutor postal address for fiscal documents.</summary>
    public Result SetAddress(PostalAddress? address)
    {
        if (IsAnonymized)
        {
            return Result.Failure(ErrorCodes.Tutor.AlreadyAnonymized);
        }

        if (IsDeleted)
        {
            return Result.Failure(ErrorCodes.Tutor.AlreadyDeleted);
        }

        Address = address;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Updates mutable tutor fields (CPF remains immutable).</summary>
    public Result Update(string name, Email email, Phone phone)
    {
        if (IsAnonymized)
        {
            return Result.Failure(ErrorCodes.Tutor.AlreadyAnonymized);
        }

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
    /// Replaces personal identifiers with deterministic tombstones (LGPD erasure); idempotent.
    /// </summary>
    public Result Anonymize()
    {
        if (IsAnonymized)
        {
            return Result.Success();
        }

        var emailResult = PersonalDataTombstone.Email(Id);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var cpfResult = PersonalDataTombstone.Cpf(Id);
        if (cpfResult.IsFailure)
        {
            return Result.Failure(cpfResult.Error);
        }

        var phoneResult = PersonalDataTombstone.Phone(Id);
        if (phoneResult.IsFailure)
        {
            return Result.Failure(phoneResult.Error);
        }

        Name = PersonalDataTombstone.Name(Id);
        Email = emailResult.Value;
        Cpf = cpfResult.Value;
        Phone = phoneResult.Value;
        Address = null;
        IsAnonymized = true;
        AnonymizedAt = DateTimeOffset.UtcNow;
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
