namespace Core.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um Pet.
/// </summary>
public class Pet : Entity, ISoftDeletable, IAuditable
{
    public string Name { get; private set; }
    public PetSpecies Species { get; private set; }
    public string Breed { get; private set; }
    public PetSex Sex { get; private set; }
    public Guid TutorId { get; private set; }

    /// <inheritdoc />
    public bool IsDeleted { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; private set; }

#pragma warning disable CS8618
    protected Pet() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

    private Pet(Guid id, string name, PetSpecies species, string breed, PetSex sex, Guid tutorId)
        : base(id)
    {
        Name = name;
        Species = species;
        Breed = breed;
        Sex = sex;
        TutorId = tutorId;
    }

    /// <summary>
    /// Factory Method para criação de um Pet com validações.
    /// </summary>
    public static Result<Pet> Create(string name, PetSpecies species, string breed, PetSex sex, Guid tutorId, Guid id = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Pet>(ErrorCodes.Pet.InvalidName);
        }

        if (tutorId == Guid.Empty)
        {
            return Result.Failure<Pet>(ErrorCodes.Pet.InvalidTutor);
        }

        if (!Enum.IsDefined(typeof(PetSpecies), species))
        {
            return Result.Failure<Pet>(ErrorCodes.Pet.InvalidSpecies);
        }

        if (!Enum.IsDefined(typeof(PetSex), sex))
        {
            return Result.Failure<Pet>(ErrorCodes.Pet.InvalidSex);
        }

        var pet = new Pet(id, name.Trim(), species, breed?.Trim() ?? string.Empty, sex, tutorId);
        return Result.Success(pet);
    }

    /// <summary>
    /// Atualiza dados do pet enquanto não estiver excluído logicamente.
    /// </summary>
    public Result Update(string name, PetSpecies species, string breed, PetSex sex)
    {
        if (IsDeleted)
        {
            return Result.Failure(ErrorCodes.Pet.AlreadyDeleted);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(ErrorCodes.Pet.InvalidName);
        }

        if (!Enum.IsDefined(typeof(PetSpecies), species))
        {
            return Result.Failure(ErrorCodes.Pet.InvalidSpecies);
        }

        if (!Enum.IsDefined(typeof(PetSex), sex))
        {
            return Result.Failure(ErrorCodes.Pet.InvalidSex);
        }

        Name = name.Trim();
        Species = species;
        Breed = breed?.Trim() ?? string.Empty;
        Sex = sex;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Marca o pet como excluído logicamente; operação idempotente.
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
}
