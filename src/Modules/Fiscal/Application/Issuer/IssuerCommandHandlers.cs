using Core.Domain;
using Fiscal.Domain.Entities;
using Fiscal.Domain.Repositories;
using Fiscal.Domain.ValueObjects;
using MediatR;

namespace Fiscal.Application.Issuer;

/// <summary>Creates or updates tenant issuer profile.</summary>
public sealed class UpsertIssuerProfileCommandHandler : IRequestHandler<UpsertIssuerProfileCommand, Result<Guid>>
{
    private readonly IIssuerProfileRepository _repository;

    public UpsertIssuerProfileCommandHandler(IIssuerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(UpsertIssuerProfileCommand request, CancellationToken cancellationToken)
    {
        var cnpjResult = FiscalCnpj.Create(request.Cnpj);
        if (cnpjResult.IsFailure)
        {
            return Result.Failure<Guid>(cnpjResult.Error);
        }

        var existing = await _repository.GetAsync(cancellationToken);
        if (existing is null)
        {
            var createResult = IssuerProfile.Create(
                request.LegalName,
                request.TradeName,
                cnpjResult.Value,
                request.StateRegistration,
                request.MunicipalRegistration,
                request.Cnae,
                request.Street,
                request.Number,
                request.Complement,
                request.District,
                request.City,
                request.State,
                request.PostalCode,
                request.IbgeCityCode,
                request.Phone,
                request.NationalServiceTaxCode,
                request.DefaultIssRate,
                request.Environment);

            if (createResult.IsFailure)
            {
                return Result.Failure<Guid>(createResult.Error);
            }

            _repository.Add(createResult.Value);
            return Result.Success(createResult.Value.Id);
        }

        var updateResult = existing.Update(
            request.LegalName,
            request.TradeName,
            request.StateRegistration,
            request.MunicipalRegistration,
            request.Cnae,
            request.Street,
            request.Number,
            request.Complement,
            request.District,
            request.City,
            request.State,
            request.PostalCode,
            request.IbgeCityCode,
            request.Phone,
            request.NationalServiceTaxCode,
            request.DefaultIssRate,
            request.Environment);

        if (updateResult.IsFailure)
        {
            return Result.Failure<Guid>(updateResult.Error);
        }

        _repository.Update(existing);
        return Result.Success(existing.Id);
    }
}

/// <summary>Returns issuer profile for tenant.</summary>
public sealed class GetIssuerProfileQueryHandler : IRequestHandler<GetIssuerProfileQuery, Result<IssuerProfileDto?>>
{
    private readonly IIssuerProfileRepository _repository;

    public GetIssuerProfileQueryHandler(IIssuerProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IssuerProfileDto?>> Handle(GetIssuerProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetAsync(cancellationToken);
        if (profile is null)
        {
            return Result.Success<IssuerProfileDto?>(null);
        }

        return Result.Success<IssuerProfileDto?>(new IssuerProfileDto(
            profile.Id,
            profile.LegalName,
            profile.TradeName,
            profile.Cnpj.Value,
            profile.StateRegistration,
            profile.MunicipalRegistration,
            profile.Cnae,
            profile.Street,
            profile.Number,
            profile.Complement,
            profile.District,
            profile.City,
            profile.State,
            profile.PostalCode,
            profile.IbgeCityCode,
            profile.Phone,
            profile.NationalServiceTaxCode,
            profile.DefaultIssRate,
            profile.TaxRegimeCode,
            profile.NfeSeries,
            profile.NextNfeNumber,
            profile.DpsSeries,
            profile.NextDpsNumber,
            profile.Environment,
            profile.HasCertificate));
    }
}
