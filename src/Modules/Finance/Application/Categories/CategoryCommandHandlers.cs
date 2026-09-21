using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Finance.Application.Categories.Dtos;
using Finance.Domain.Entities;
using Finance.Domain.Repositories;

namespace Finance.Application.Categories;

public sealed class ListFinancialCategoriesQueryHandler : IRequestHandler<ListFinancialCategoriesQuery, Result<IReadOnlyList<FinancialCategoryDto>>>
{
    private readonly IFinancialCategoryRepository _repository;

    public ListFinancialCategoriesQueryHandler(IFinancialCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<FinancialCategoryDto>>> Handle(ListFinancialCategoriesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<FinancialCategoryDto>>(items.Select(c => new FinancialCategoryDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Direction = c.Direction,
            IsSystem = c.IsSystem,
            IsActive = c.IsActive
        }).ToList());
    }
}

public sealed class UpsertFinancialCategoryCommandHandler : IRequestHandler<UpsertFinancialCategoryCommand, Result<Guid>>
{
    private readonly IFinancialCategoryRepository _repository;

    public UpsertFinancialCategoryCommandHandler(IFinancialCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(UpsertFinancialCategoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is not null)
        {
            var update = existing.Update(request.Name, request.Direction, request.IsActive);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            _repository.Update(existing);
            return Result.Success(existing.Id);
        }

        var created = FinancialCategory.Create(request.Code, request.Name, request.Direction, request.Id);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _repository.Add(created.Value);
        return Result.Success(created.Value.Id);
    }
}
