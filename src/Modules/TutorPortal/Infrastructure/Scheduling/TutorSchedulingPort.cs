using Core.Application.Common.Interfaces;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Petshop.Application.GroomingAppointments;
using Petshop.Domain.Enums;
using Petshop.Domain.Repositories;
using Petshop.Infrastructure.Persistence;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Scheduling;
using TutorPortal.Application.Scheduling.Dtos;
using TutorPortalErrorCodes = TutorPortal.Domain.ErrorCodes;
using Veterinary.Application.Appointments;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;
using Veterinary.Infrastructure.Persistence;

namespace TutorPortal.Infrastructure.Scheduling;

/// <summary>
/// Aggregates tutor booking operations across Veterinary and Petshop modules.
/// </summary>
public sealed class TutorSchedulingPort : ITutorSchedulingPort
{
    private readonly IAppointmentScheduler _clinicalScheduler;
    private readonly IGroomingAppointmentScheduler _groomingScheduler;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IGroomingAppointmentRepository _groomingAppointmentRepository;
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IGroomingSlotRepository _groomingSlotRepository;
    private readonly IGroomingServiceRepository _groomingServiceRepository;
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;
    private readonly VeterinaryDbContext _veterinaryDbContext;
    private readonly PetshopDbContext _petshopDbContext;

    /// <summary>Creates the port with cross-module scheduling dependencies.</summary>
    public TutorSchedulingPort(
        IAppointmentScheduler clinicalScheduler,
        IGroomingAppointmentScheduler groomingScheduler,
        IAppointmentRepository appointmentRepository,
        IGroomingAppointmentRepository groomingAppointmentRepository,
        IScheduleSlotRepository scheduleSlotRepository,
        IGroomingSlotRepository groomingSlotRepository,
        IGroomingServiceRepository groomingServiceRepository,
        IIdentityService identityService,
        ITenantContext tenantContext,
        VeterinaryDbContext veterinaryDbContext,
        PetshopDbContext petshopDbContext)
    {
        _clinicalScheduler = clinicalScheduler;
        _groomingScheduler = groomingScheduler;
        _appointmentRepository = appointmentRepository;
        _groomingAppointmentRepository = groomingAppointmentRepository;
        _scheduleSlotRepository = scheduleSlotRepository;
        _groomingSlotRepository = groomingSlotRepository;
        _groomingServiceRepository = groomingServiceRepository;
        _identityService = identityService;
        _tenantContext = tenantContext;
        _veterinaryDbContext = veterinaryDbContext;
        _petshopDbContext = petshopDbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorBookableServiceDto>> ListServicesAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<TutorBookableServiceDto>
        {
            new()
            {
                ServiceId = TutorBookingConstants.ClinicalConsultationServiceId,
                Name = TutorBookingConstants.ClinicalConsultationName,
                Kind = TutorBookingKind.Clinical.ToString(),
                DurationInMinutes = TutorBookingConstants.ClinicalConsultationDurationMinutes
            }
        };

        var grooming = await _groomingServiceRepository.ListActiveAsync(cancellationToken);
        list.AddRange(grooming.Select(s => new TutorBookableServiceDto
        {
            ServiceId = s.Id,
            Name = s.Name,
            Kind = TutorBookingKind.Grooming.ToString(),
            DurationInMinutes = s.DurationInMinutes
        }));

        return list;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorBookingProfessionalDto>> ListProfessionalsAsync(
        TutorBookingKind kind,
        Guid serviceId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        var durationResult = await ResolveDurationAsync(kind, serviceId, cancellationToken);
        if (durationResult.IsFailure)
        {
            return Array.Empty<TutorBookingProfessionalDto>();
        }

        var duration = durationResult.Value;

        var day = date.Date;
        var nextDay = day.AddDays(1);

        List<Guid> professionalIds;
        if (kind == TutorBookingKind.Clinical)
        {
            professionalIds = await _veterinaryDbContext.ScheduleSlots.AsNoTracking()
                .Where(s => s.Date >= day && s.Date < nextDay)
                .Select(s => s.VeterinarianId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else
        {
            professionalIds = await _petshopDbContext.GroomingSlots.AsNoTracking()
                .Where(s => s.Date >= day && s.Date < nextDay)
                .Select(s => s.GroomerId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        var tenantId = _tenantContext.TenantId;
        var results = new List<TutorBookingProfessionalDto>();
        foreach (var professionalId in professionalIds)
        {
            var hasBookable = kind == TutorBookingKind.Clinical
                ? HasBookableWindow(
                    (await _scheduleSlotRepository.GetAvailableSlotsAsync(professionalId, date, cancellationToken))
                    .Select(s => (s.StartTime, s.EndTime)),
                    duration)
                : HasBookableWindow(
                    (await _groomingSlotRepository.GetAvailableSlotsAsync(professionalId, date, cancellationToken))
                    .Select(s => (s.StartTime, s.EndTime)),
                    duration);

            if (!hasBookable)
            {
                continue;
            }

            var name = await ResolveDisplayNameAsync(professionalId, tenantId, cancellationToken);
            results.Add(new TutorBookingProfessionalDto
            {
                ProfessionalId = professionalId,
                DisplayName = name
            });
        }

        return results.OrderBy(p => p.DisplayName).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorAvailableSlotDto>> ListAvailableSlotsAsync(
        TutorBookingKind kind,
        Guid serviceId,
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        var durationResult = await ResolveDurationAsync(kind, serviceId, cancellationToken);
        if (durationResult.IsFailure)
        {
            return Array.Empty<TutorAvailableSlotDto>();
        }

        var duration = durationResult.Value;

        if (kind == TutorBookingKind.Clinical)
        {
            var slots = await _scheduleSlotRepository.GetAvailableSlotsAsync(professionalId, date, cancellationToken);
            return BuildSlotDtos(slots.Select(s => (s.Date, s.StartTime, s.EndTime)), duration);
        }

        var groomingSlots = await _groomingSlotRepository.GetAvailableSlotsAsync(professionalId, date, cancellationToken);
        return BuildSlotDtos(groomingSlots.Select(s => (s.Date, s.StartTime, s.EndTime)), duration);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> BookAsync(
        Guid appointmentId,
        Guid tutorId,
        Guid petId,
        TutorBookingKind kind,
        Guid serviceId,
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default)
    {
        if (kind == TutorBookingKind.Clinical)
        {
            if (serviceId != TutorBookingConstants.ClinicalConsultationServiceId)
            {
                return Result.Failure<Guid>(TutorPortalErrorCodes.Booking.InvalidService);
            }

            var result = await _clinicalScheduler.ScheduleAsync(
                appointmentId,
                tutorId,
                petId,
                professionalId,
                date,
                TutorBookingConstants.ClinicalConsultationDurationMinutes,
                TutorBookingConstants.PortalBookingText,
                cancellationToken);

            return MapScheduleResult(result);
        }

        if (kind != TutorBookingKind.Grooming)
        {
            return Result.Failure<Guid>(TutorPortalErrorCodes.Booking.InvalidRequest);
        }

        var groomingResult = await _groomingScheduler.ScheduleAsync(
            appointmentId,
            tutorId,
            petId,
            professionalId,
            serviceId,
            date,
            0,
            TutorBookingConstants.PortalBookingText,
            cancellationToken);

        return MapScheduleResult(groomingResult);
    }

    /// <inheritdoc />
    public async Task<Result> CancelAsync(
        Guid tutorId,
        Guid petId,
        TutorBookingKind kind,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        if (kind == TutorBookingKind.Clinical)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment is null || appointment.TutorId != tutorId || appointment.PetId != petId)
            {
                return Result.Failure(TutorPortalErrorCodes.Booking.NotFound);
            }

            return MapCancelResult(await _clinicalScheduler.CancelAndReleaseSlotAsync(appointmentId, cancellationToken));
        }

        var grooming = await _groomingAppointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (grooming is null || grooming.TutorId != tutorId || grooming.PetId != petId)
        {
            return Result.Failure(TutorPortalErrorCodes.Booking.NotFound);
        }

        return MapCancelResult(await _groomingScheduler.CancelAndReleaseSlotAsync(appointmentId, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TutorPetAppointmentDto>> ListAppointmentsAsync(
        Guid tutorId,
        Guid petId,
        CancellationToken cancellationToken = default)
    {
        var items = new List<TutorPetAppointmentDto>();

        var clinical = await _veterinaryDbContext.Appointments.AsNoTracking()
            .Where(a => a.TutorId == tutorId && a.PetId == petId)
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
            .ToListAsync(cancellationToken);

        foreach (var a in clinical)
        {
            items.Add(new TutorPetAppointmentDto
            {
                Id = a.Id,
                Kind = TutorBookingKind.Clinical.ToString(),
                Date = a.Date,
                Status = a.Status.ToString(),
                Title = string.IsNullOrWhiteSpace(a.Reason) ? TutorBookingConstants.ClinicalConsultationName : a.Reason,
                CanCancel = a.Status is AppointmentStatus.Scheduled or AppointmentStatus.Confirmed
            });
        }

        var groomingRows = await _petshopDbContext.GroomingAppointments.AsNoTracking()
            .Where(a => a.TutorId == tutorId && a.PetId == petId)
            .Where(a => a.Status != GroomingAppointmentStatus.Cancelled && a.Status != GroomingAppointmentStatus.NoShow)
            .ToListAsync(cancellationToken);

        var serviceIds = groomingRows.Select(g => g.GroomingServiceId).Distinct().ToList();
        var services = await _petshopDbContext.GroomingServices.AsNoTracking()
            .Where(s => serviceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        foreach (var g in groomingRows)
        {
            var title = services.TryGetValue(g.GroomingServiceId, out var name) ? name : "Banho e tosa";
            items.Add(new TutorPetAppointmentDto
            {
                Id = g.Id,
                Kind = TutorBookingKind.Grooming.ToString(),
                Date = g.Date,
                Status = g.Status.ToString(),
                Title = title,
                CanCancel = g.Status is GroomingAppointmentStatus.Scheduled or GroomingAppointmentStatus.Confirmed
            });
        }

        items.Sort((a, b) => a.Date.CompareTo(b.Date));
        return items;
    }

    private async Task<Result<int>> ResolveDurationAsync(
        TutorBookingKind kind,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        if (kind == TutorBookingKind.Clinical)
        {
            if (serviceId != TutorBookingConstants.ClinicalConsultationServiceId)
            {
                return Result.Failure<int>(TutorPortalErrorCodes.Booking.InvalidService);
            }

            return Result.Success(TutorBookingConstants.ClinicalConsultationDurationMinutes);
        }

        var service = await _groomingServiceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service is null || !service.IsActive)
        {
            return Result.Failure<int>(TutorPortalErrorCodes.Booking.InvalidService);
        }

        return Result.Success(service.DurationInMinutes);
    }

    private async Task<string> ResolveDisplayNameAsync(Guid professionalId, Guid tenantId, CancellationToken cancellationToken)
    {
        var userId = professionalId.ToString();
        var userResult = await _identityService.GetByIdAsync(userId, tenantId, cancellationToken);
        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Value.DisplayName))
        {
            return userResult.Value.DisplayName;
        }

        if (userResult.IsSuccess && !string.IsNullOrWhiteSpace(userResult.Value.Email))
        {
            return userResult.Value.Email;
        }

        return "Profissional";
    }

    private static bool HasBookableWindow(IEnumerable<(TimeSpan Start, TimeSpan End)> windows, int durationMinutes)
    {
        var duration = TimeSpan.FromMinutes(durationMinutes);
        return windows.Any(w => w.End - w.Start >= duration);
    }

    private static IReadOnlyList<TutorAvailableSlotDto> BuildSlotDtos(
        IEnumerable<(DateTimeOffset Date, TimeSpan Start, TimeSpan End)> slots,
        int durationMinutes)
    {
        var duration = TimeSpan.FromMinutes(durationMinutes);
        var list = new List<TutorAvailableSlotDto>();
        foreach (var slot in slots.OrderBy(s => s.Start))
        {
            if (slot.End - slot.Start < duration)
            {
                continue;
            }

            var start = new DateTimeOffset(slot.Date.DateTime.Date, slot.Date.Offset).Add(slot.Start);
            list.Add(new TutorAvailableSlotDto
            {
                Start = start,
                DurationInMinutes = durationMinutes
            });
        }

        return list;
    }

    private static Result<Guid> MapScheduleResult(Result<Guid> result)
    {
        if (result.IsSuccess)
        {
            return result;
        }

        return result.Error.Code switch
        {
            "Appointment.SlotUnavailable" or "GroomingAppointment.SlotUnavailable" => Result.Failure<Guid>(TutorPortalErrorCodes.Booking.SlotUnavailable),
            "Appointment.Overlap" or "GroomingAppointment.Overlap" => Result.Failure<Guid>(TutorPortalErrorCodes.Booking.Overlap),
            "GroomingService.NotFound" => Result.Failure<Guid>(TutorPortalErrorCodes.Booking.InvalidService),
            _ => Result.Failure<Guid>(result.Error)
        };
    }

    private static Result MapCancelResult(Result result)
    {
        if (result.IsSuccess)
        {
            return result;
        }

        return result.Error.Code switch
        {
            "Appointment.NotFound" or "GroomingAppointment.NotFound" => Result.Failure(TutorPortalErrorCodes.Booking.NotFound),
            "Appointment.InvalidStatusTransition" or "GroomingAppointment.InvalidStatusTransition" => Result.Failure(TutorPortalErrorCodes.Booking.InvalidRequest),
            _ => result
        };
    }
}
