using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// Soft-deletes a tutor aggregate and cascades soft delete to linked pets.
/// </summary>
public class DeleteTutorCommandHandler : IRequestHandler<DeleteTutorCommand, Result>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IPetRepository _petRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTutorCommandHandler"/> class.
    /// </summary>
    public DeleteTutorCommandHandler(ITutorRepository tutorRepository, IPetRepository petRepository)
    {
        _tutorRepository = tutorRepository;
        _petRepository = petRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteTutorCommand request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NotFound);
        }

        tutor.SoftDelete();

        var pets = await _petRepository.GetByTutorIdAsync(request.Id, cancellationToken);
        foreach (var pet in pets)
        {
            pet.SoftDelete();
            _petRepository.Update(pet);
        }

        _tutorRepository.Update(tutor);

        return Result.Success();
    }
}
