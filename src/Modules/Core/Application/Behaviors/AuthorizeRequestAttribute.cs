namespace Core.Application.Behaviors;

/// <summary>
/// Declares the authorization policy required before the handler runs.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuthorizeRequestAttribute(string policy) : Attribute
{
    /// <summary>
    /// Policy name registered in dependency injection (e.g. ClinicStaff, Admin).
    /// </summary>
    public string Policy { get; } = policy;
}
