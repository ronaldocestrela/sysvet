using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using MediatR;

namespace Core.Application.Tutors.Commands;

public class RegisterTutorCommandHandler : IRequestHandler<RegisterTutorCommand, Result<Guid>>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterTutorCommandHandler(ITutorRepository tutorRepository, IUnitOfWork unitOfWork)
    {
        _tutorRepository = tutorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterTutorCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return Result.Failure<Guid>(emailResult.Error);

        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure) return Result.Failure<Guid>(cpfResult.Error);

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure) return Result.Failure<Guid>(phoneResult.Error);

        var tutorResult = Tutor.Create(request.Name, emailResult.Value, cpfResult.Value, phoneResult.Value);
        if (tutorResult.IsFailure) return Result.Failure<Guid>(tutorResult.Error);

        _tutorRepository.Add(tutorResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tutorResult.Value.Id);
    }
}
