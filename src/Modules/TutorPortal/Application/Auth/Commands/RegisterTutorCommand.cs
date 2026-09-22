using Core.Application.Auth.Dtos;
using Core.Application.Messaging;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Self-registration for tutors when email and CPF match an active CRM record.
/// </summary>
public sealed record RegisterTutorCommand(string Email, string Cpf, string Password) : ICommand<AuthTokensDto>;
