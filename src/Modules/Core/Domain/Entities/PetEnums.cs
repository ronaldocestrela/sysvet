namespace Core.Domain.Entities;

/// <summary>
/// Espécie do animal atendido na clínica.
/// </summary>
public enum PetSpecies
{
    Dog = 1,
    Cat = 2,
    Bird = 3,
    Reptile = 4,
    Other = 99
}

/// <summary>
/// Sexo do animal.
/// </summary>
public enum PetSex
{
    Male = 1,
    Female = 2,
    Unknown = 3
}
