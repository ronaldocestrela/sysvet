namespace Core.Application.Behaviors;

/// <summary>
/// Declares the authorization policy required before the handler runs.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuthorizeRequestAttribute(string policy, string? permission = null) : Attribute
{
    /// <summary>
    /// Policy name registered in dependency injection (e.g. ClinicStaff, Admin).
    /// </summary>
    public string Policy { get; } = policy;

    /// <summary>
    /// Optional fine-grained permission from <see cref="Core.Domain.Authorization.Permissions"/> required in addition to the policy.
    /// </summary>
    public string? Permission { get; } = permission;
}
