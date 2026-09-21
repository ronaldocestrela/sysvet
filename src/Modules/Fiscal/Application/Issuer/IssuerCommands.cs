using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Fiscal.Domain.Enums;

namespace Fiscal.Application.Issuer;

[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.FiscalWrite)]
public sealed record UpsertIssuerProfileCommand(
    string LegalName,
    string TradeName,
    string Cnpj,
    string StateRegistration,
    string MunicipalRegistration,
    string Cnae,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string PostalCode,
    int IbgeCityCode,
    string Phone,
    string NationalServiceTaxCode,
    decimal DefaultIssRate,
    FiscalEnvironment Environment) : ICommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalRead)]
public sealed record GetIssuerProfileQuery : IQuery<IssuerProfileDto?>;
