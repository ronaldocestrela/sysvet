using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedUI.Services;

public class MockVeterinaryApiService : IVeterinaryApiService
{
    public async Task<List<AppointmentDto>> GetDailyAppointmentsAsync(DateTimeOffset date)
    {
        await Task.Delay(500); // Simulate network
        return new List<AppointmentDto>
        {
            new AppointmentDto { Id = Guid.NewGuid(), PetId = Guid.NewGuid(), PetName = "Rex", VetName = "Dra. Ana", SlotTime = date.Date.AddHours(9), Reason = "Rotina", Status = "Scheduled" },
            new AppointmentDto { Id = Guid.NewGuid(), PetId = Guid.NewGuid(), PetName = "Mia", VetName = "Dr. Carlos", SlotTime = date.Date.AddHours(14), Reason = "Vacina", Status = "Scheduled" }
        };
    }

    public async Task<bool> ScheduleAppointmentAsync(Guid petId, Guid vetId, DateTimeOffset slot, string reason)
    {
        await Task.Delay(500);
        return true;
    }

    public async Task<List<HospitalizationDto>> GetActiveHospitalizationsAsync()
    {
        await Task.Delay(500);
        return new List<HospitalizationDto>
        {
            new HospitalizationDto { Id = Guid.NewGuid(), PetId = Guid.NewGuid(), PetName = "Thor", Reason = "Cirurgia ortopédica", AdmittedAt = DateTimeOffset.UtcNow.AddDays(-2), Status = "Admitted" }
        };
    }

    public async Task<bool> AdmitPetAsync(Guid petId, Guid vetId, string reason)
    {
        await Task.Delay(500);
        return true;
    }

    public async Task<bool> DischargePetAsync(Guid hospitalizationId)
    {
        await Task.Delay(500);
        return true;
    }

    public async Task<bool> ExecutePrescriptionAsync(Guid hospitalizationId, string medication, string dose, string notes, Guid executedBy)
    {
        await Task.Delay(500);
        return true;
    }

    public async Task<List<MedicalRecordDto>> GetPetMedicalRecordsAsync(Guid petId)
    {
        await Task.Delay(500);
        return new List<MedicalRecordDto>();
    }
}
