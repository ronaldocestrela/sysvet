using Core.Domain;
using MediatR;

using Core.Application.Common;

namespace Core.Application.Tutors.Commands;

public record UpdateTutorCommand(Guid Id, string Name, string Email, string Cpf, string Phone, Guid IdempotencyKey = default) : IIdempotentCommand<Result>;
