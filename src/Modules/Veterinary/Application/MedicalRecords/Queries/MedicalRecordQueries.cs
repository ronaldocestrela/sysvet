using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Veterinary.Application.MedicalRecords.DTOs;

namespace Veterinary.Application.MedicalRecords.Queries;

/// <summary>Loads a medical record by identifier.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public record GetMedicalRecordByIdQuery(Guid Id) : IQuery<MedicalRecordDto>;

/// <summary>Loads the medical record linked to an appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public record GetMedicalRecordByAppointmentQuery(Guid AppointmentId) : IQuery<MedicalRecordDto>;

/// <summary>Lists clinical timeline entries for a pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsRead)]
public record GetPetClinicalTimelineQuery(Guid PetId) : IQuery<IReadOnlyList<PetClinicalTimelineItemDto>>;
