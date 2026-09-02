using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedUI.Services;

public interface IVeterinaryApiService
{
    Task<List<AppointmentDto>> GetDailyAppointmentsAsync(DateTimeOffset date);
    Task<bool> ScheduleAppointmentAsync(Guid petId, Guid vetId, DateTimeOffset slot, string reason);
    
    Task<List<HospitalizationDto>> GetActiveHospitalizationsAsync();
    Task<bool> AdmitPetAsync(Guid petId, Guid vetId, string reason);
    Task<bool> DischargePetAsync(Guid hospitalizationId);
    Task<bool> ExecutePrescriptionAsync(Guid hospitalizationId, string medication, string dose, string notes, Guid executedBy);

    Task<List<MedicalRecordDto>> GetPetMedicalRecordsAsync(Guid petId);
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public Guid VeterinarianId { get; set; }
    public DateTimeOffset SlotTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PetName { get; set; } = string.Empty;
    public string VetName { get; set; } = string.Empty;
}

public class HospitalizationDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MedicalRecordDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
