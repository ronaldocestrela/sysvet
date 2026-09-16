namespace Clients.Infrastructure.Http;

/// <summary>CRM tutor row returned by the API.</summary>
public sealed class TutorDto
{
    /// <summary>Tutor identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Contact e-mail.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>CPF (Brazilian tax id).</summary>
    public string Cpf { get; set; } = string.Empty;

    /// <summary>Phone number.</summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>Request body for creating a tutor.</summary>
public sealed class CreateTutorRequest
{
    /// <summary>Client-generated id for idempotent creates.</summary>
    public Guid Id { get; set; }

    /// <summary>Full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Contact e-mail.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>CPF.</summary>
    public string Cpf { get; set; } = string.Empty;

    /// <summary>Phone number.</summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>Request body for updating a tutor (CPF is immutable server-side).</summary>
public sealed class UpdateTutorRequest
{
    /// <summary>Tutor identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Full name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Contact e-mail.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Phone number.</summary>
    public string Phone { get; set; } = string.Empty;
}

/// <summary>Pet species values aligned with Core domain enums (numeric JSON).</summary>
public enum PetSpeciesDto
{
    Dog = 1,
    Cat = 2,
    Bird = 3,
    Reptile = 4,
    Other = 99
}

/// <summary>Pet sex values aligned with Core domain enums (numeric JSON).</summary>
public enum PetSexDto
{
    Male = 1,
    Female = 2,
    Unknown = 3
}

/// <summary>CRM pet row returned by the API.</summary>
public sealed class PetDto
{
    /// <summary>Pet identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Pet name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Species.</summary>
    public PetSpeciesDto Species { get; set; }

    /// <summary>Breed.</summary>
    public string Breed { get; set; } = string.Empty;

    /// <summary>Sex.</summary>
    public PetSexDto Sex { get; set; }

    /// <summary>Owning tutor id.</summary>
    public Guid TutorId { get; set; }
}

/// <summary>Request body for creating a pet.</summary>
public sealed class CreatePetRequest
{
    /// <summary>Client-generated id for idempotent creates.</summary>
    public Guid Id { get; set; }

    /// <summary>Pet name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Species.</summary>
    public PetSpeciesDto Species { get; set; }

    /// <summary>Breed.</summary>
    public string Breed { get; set; } = string.Empty;

    /// <summary>Sex.</summary>
    public PetSexDto Sex { get; set; }

    /// <summary>Owning tutor id.</summary>
    public Guid TutorId { get; set; }
}

/// <summary>Request body for updating a pet.</summary>
public sealed class UpdatePetRequest
{
    /// <summary>Pet identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Pet name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Species.</summary>
    public PetSpeciesDto Species { get; set; }

    /// <summary>Breed.</summary>
    public string Breed { get; set; } = string.Empty;

    /// <summary>Sex.</summary>
    public PetSexDto Sex { get; set; }
}

/// <summary>Authentication tokens from login or refresh.</summary>
public sealed class AuthTokensDto
{
    /// <summary>JWT access token.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Opaque refresh token.</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Access token lifetime in seconds.</summary>
    public int ExpiresInSeconds { get; set; }
}

/// <summary>Login request body.</summary>
public sealed class LoginRequest
{
    /// <summary>User e-mail.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Password.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>Refresh token request body.</summary>
public sealed class RefreshTokenRequest
{
    /// <summary>Current refresh token.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>Authenticated user profile from GET /api/v1/auth/me.</summary>
public sealed class CurrentUserDto
{
    /// <summary>User id.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>E-mail.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Tenant id.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Identity roles.</summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>Access profile id.</summary>
    public Guid ProfileId { get; set; }

    /// <summary>Access profile display name.</summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>Granted permission codes.</summary>
    public IReadOnlyList<string> Permissions { get; set; } = [];

    /// <summary>Menu keys the user may see.</summary>
    public IReadOnlyList<string> Menus { get; set; } = [];
}
