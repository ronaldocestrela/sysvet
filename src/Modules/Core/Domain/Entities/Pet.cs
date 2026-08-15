namespace Core.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um Pet.
/// </summary>
public class Pet : Entity
{
    public string Name { get; private set; }
    public PetSpecies Species { get; private set; }
    public string Breed { get; private set; }
    public PetSex Sex { get; private set; }
    public Guid TutorId { get; private set; }

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
            return Result.Failure<Pet>(new Error("Pet.InvalidName", "O nome do pet não pode ser vazio."));
        }

        if (tutorId == Guid.Empty)
        {
            return Result.Failure<Pet>(new Error("Pet.InvalidTutor", "O pet deve ser associado a um tutor válido."));
        }

        var pet = new Pet(id, name.Trim(), species, breed?.Trim() ?? string.Empty, sex, tutorId);
        return Result.Success(pet);
    }
}
