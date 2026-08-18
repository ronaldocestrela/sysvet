using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Commands;

public record UpdateTutorCommand(Guid Id, string Name, string Email, string Cpf, string Phone) : IRequest<Result>;
