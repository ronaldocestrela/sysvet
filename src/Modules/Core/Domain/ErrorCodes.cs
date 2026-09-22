namespace Core.Domain;

/// <summary>
/// Standardized error codes for the Core module ({Aggregate}.{Reason}).
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Authentication failures for login, register, and refresh flows.
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Email/password combination is invalid (same code whether the user exists or not).
        /// </summary>
        public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.");

        /// <summary>
        /// The account is temporarily locked after repeated failed sign-in attempts.
        /// </summary>
        public static readonly Error LockedOut = new("Auth.LockedOut", "This account is temporarily locked. Try again later.");

        /// <summary>
        /// A user with the same email already exists.
        /// </summary>
        public static readonly Error DuplicateEmail = new("Auth.DuplicateEmail", "A user with this email already exists.");

        /// <summary>
        /// The requested role is not a valid application role.
        /// </summary>
        public static readonly Error InvalidRole = new("Auth.InvalidRole", "The specified role is not valid.");

        /// <summary>
        /// The refresh token is missing, expired, revoked, or already rotated.
        /// </summary>
        public static readonly Error InvalidRefreshToken = new("Auth.InvalidRefreshToken", "The refresh token is invalid or expired.");

        /// <summary>
        /// User registration is only allowed in the Development environment.
        /// </summary>
        public static readonly Error RegistrationNotAllowed = new("Auth.RegistrationNotAllowed", "Registration is not available in this environment.");

        /// <summary>
        /// The account has been disabled by an administrator.
        /// </summary>
        public static readonly Error Disabled = new("Auth.Disabled", "This account has been disabled.");

        /// <summary>
        /// The tenant account is suspended, cancelled, or deleted.
        /// </summary>
        public static readonly Error TenantNotActive = new("Auth.TenantNotActive", "This clinic account is not active.");

        /// <summary>
        /// Valid credentials were presented on the wrong login surface (clinic vs tutor portal).
        /// </summary>
        public static readonly Error WrongPortal = new("Auth.WrongPortal", "Use the clinic application to sign in with this account.");
    }

    /// <summary>
    /// Cross-cutting authorization failures surfaced by the application pipeline.
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// The caller is not authenticated.
        /// </summary>
        public static readonly Error Unauthorized = new("Authorization.Unauthorized", "Authentication is required.");

        /// <summary>
        /// The caller is authenticated but lacks the required policy.
        /// </summary>
        public static readonly Error Forbidden = new("Authorization.Forbidden", "You do not have permission to perform this action.");
    }

    /// <summary>
    /// FluentValidation and pipeline validation failures.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// One or more validation rules failed.
        /// </summary>
        public static readonly Error Error = new("Validation.Error", "A validation error occurred.");
    }

    /// <summary>
    /// Tutor aggregate errors.
    /// </summary>
    public static class Tutor
    {
        public static readonly Error NotFound = new("Tutor.NotFound", "O tutor especificado não foi encontrado.");
        public static readonly Error InvalidName = new("Tutor.InvalidName", "O nome do tutor deve ter pelo menos 2 caracteres.");
        public static readonly Error NullEmail = new("Tutor.NullEmail", "O e-mail é obrigatório.");
        public static readonly Error NullCpf = new("Tutor.NullCpf", "O CPF é obrigatório.");
        public static readonly Error NullPhone = new("Tutor.NullPhone", "O telefone é obrigatório.");
        public static readonly Error NullPet = new("Tutor.NullPet", "Não é possível adicionar um pet nulo.");
        public static readonly Error InvalidCpf = new("Tutor.InvalidCpf", "O CPF fornecido é inválido ou já está em uso.");
        public static readonly Error DuplicateCpf = new("Tutor.DuplicateCpf", "Já existe um tutor cadastrado com este CPF.");
        public static readonly Error DuplicateEmail = new("Tutor.DuplicateEmail", "Já existe um tutor cadastrado com este e-mail.");
        public static readonly Error AlreadyDeleted = new("Tutor.AlreadyDeleted", "O tutor foi excluído e não pode ser alterado.");
    }

    /// <summary>
    /// Pet entity errors.
    /// </summary>
    public static class Pet
    {
        public static readonly Error NotFound = new("Pet.NotFound", "O pet especificado não foi encontrado.");
        public static readonly Error InvalidName = new("Pet.InvalidName", "O nome do pet não pode ser vazio.");
        public static readonly Error InvalidTutor = new("Pet.InvalidTutor", "O pet deve ser associado a um tutor válido.");
        public static readonly Error TutorNotFound = new("Pet.TutorNotFound", "O tutor associado ao pet não foi encontrado.");
        public static readonly Error InvalidSpecies = new("Pet.InvalidSpecies", "A espécie do pet é obrigatória e deve ser válida.");
        public static readonly Error InvalidSex = new("Pet.InvalidSex", "O sexo do pet deve ser válido.");
        public static readonly Error TutorInactive = new("Pet.TutorInactive", "Não é possível vincular pets a um tutor inativo.");
        public static readonly Error AlreadyDeleted = new("Pet.AlreadyDeleted", "O pet foi excluído e não pode ser alterado.");
        public static readonly Error InvalidBirthDate = new("Pet.InvalidBirthDate", "A data de nascimento do pet não pode ser futura.");
    }

    /// <summary>
    /// CPF value object errors.
    /// </summary>
    public static class Cpf
    {
        public static readonly Error InvalidFormat = new("Cpf.InvalidFormat", "CPF inválido.");
    }

    /// <summary>
    /// Email value object errors.
    /// </summary>
    public static class Email
    {
        public static readonly Error InvalidFormat = new("Email.InvalidFormat", "E-mail inválido.");
    }

    /// <summary>
    /// Phone value object errors.
    /// </summary>
    public static class Phone
    {
        public static readonly Error InvalidFormat = new("Phone.InvalidFormat", "Telefone inválido.");
    }

    /// <summary>
    /// Access profile aggregate errors.
    /// </summary>
    public static class AccessProfile
    {
        public static readonly Error NotFound = new("AccessProfile.NotFound", "The access profile was not found.");
        public static readonly Error InvalidName = new("AccessProfile.InvalidName", "Profile name is required.");
        public static readonly Error InvalidBaseRole = new("AccessProfile.InvalidBaseRole", "Base role is required.");
        public static readonly Error InvalidPermission = new("AccessProfile.InvalidPermission", "The permission code is not in the catalog.");
        public static readonly Error DuplicateName = new("AccessProfile.DuplicateName", "A profile with this name already exists.");
        public static readonly Error CannotDeleteSystem = new("AccessProfile.CannotDeleteSystem", "System profiles cannot be deleted.");
        public static readonly Error CannotRenameSystem = new("AccessProfile.CannotRenameSystem", "System profiles cannot be renamed.");
        public static readonly Error InUse = new("AccessProfile.InUse", "The profile is assigned to users and cannot be deleted.");
        public static readonly Error InvalidMaxDiscountPercent = new("AccessProfile.InvalidMaxDiscountPercent", "Max discount percent must be between 0 and 100.");
    }

    /// <summary>
    /// Staff user account errors (Identity-backed, not a domain aggregate).
    /// </summary>
    public static class UserAccount
    {
        public static readonly Error NotFound = new("UserAccount.NotFound", "The user was not found.");
        public static readonly Error LastAdmin = new("UserAccount.LastAdmin", "Cannot disable or demote the last administrator in the tenant.");
        public static readonly Error ProfileNotFound = new("UserAccount.ProfileNotFound", "The access profile was not found.");
        public static readonly Error ProfileWrongTenant = new("UserAccount.ProfileWrongTenant", "The access profile does not belong to this tenant.");
    }

    /// <summary>
    /// User UI preference errors.
    /// </summary>
    public static class UserPreference
    {
        public static readonly Error InvalidUserId = new("UserPreference.InvalidUserId", "User id is required.");
        public static readonly Error PayloadTooLarge = new("UserPreference.PayloadTooLarge", "Preference payload exceeds the maximum allowed size.");
        public static readonly Error NotFound = new("UserPreference.NotFound", "Preferences were not found.");
    }

    /// <summary>
    /// Audit log entity errors.
    /// </summary>
    public static class AuditLog
    {
        public static readonly Error InvalidEntityName = new("AuditLog.InvalidEntityName", "O nome da entidade é obrigatório.");
        public static readonly Error InvalidAction = new("AuditLog.InvalidAction", "A ação de auditoria é obrigatória.");
    }

    /// <summary>
    /// HTTP/API request shape errors mapped to Problem Details.
    /// </summary>
    public static class Request
    {
        /// <summary>
        /// Route identifier does not match the command or body identifier.
        /// </summary>
        public static readonly Error RouteIdMismatch = new("Request.RouteIdMismatch", "O ID da rota difere do ID do comando.");
    }
}
