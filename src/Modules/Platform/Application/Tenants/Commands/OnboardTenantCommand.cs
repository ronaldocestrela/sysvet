using Core.Application.Messaging;
using Platform.Application.Tenants.Dtos;

namespace Platform.Application.Tenants.Commands;

/// <summary>Creates a tenant, provisions seed data, and creates the clinic admin user.</summary>
public sealed record OnboardTenantCommand(
    string Slug,
    string DisplayName,
    string AdminEmail,
    string AdminPassword,
    string HeadquartersCnpj,
    string HeadquartersLegalName) : ICommand<OnboardTenantResultDto>;
