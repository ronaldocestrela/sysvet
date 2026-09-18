namespace Clients.Infrastructure.Sync;

/// <summary>
/// JSON payloads aligned with Core.Application CRM command records for sync push.
/// </summary>
internal static class OutboxPayloadFactory
{
    public static string CreateTutor(Guid id, string name, string email, string cpf, string phone, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Email = email,
            Cpf = cpf,
            Phone = phone,
            IdempotencyKey = idempotencyKey
        });

    public static string UpdateTutor(Guid id, string name, string email, string phone, DateTimeOffset occurredAt, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Email = email,
            Phone = phone,
            OccurredAt = occurredAt,
            IdempotencyKey = idempotencyKey
        });

    public static string DeleteTutor(Guid id, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { Id = id, IdempotencyKey = idempotencyKey });

    public static string CreatePet(Guid id, string name, string species, string breed, string sex, Guid tutorId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            Sex = sex,
            TutorId = tutorId,
            IdempotencyKey = idempotencyKey
        });

    public static string UpdatePet(Guid id, string name, string species, string breed, string sex, DateTimeOffset occurredAt, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            Sex = sex,
            OccurredAt = occurredAt,
            IdempotencyKey = idempotencyKey
        });

    public static string DeletePet(Guid id, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { Id = id, IdempotencyKey = idempotencyKey });

    public static string ScheduleAppointment(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid veterinarianId,
        DateTimeOffset date,
        int durationInMinutes,
        string reason,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            TutorId = tutorId,
            PetId = petId,
            VeterinarianId = veterinarianId,
            Date = date,
            DurationInMinutes = durationInMinutes,
            Reason = reason,
            IdempotencyKey = idempotencyKey
        });

    public static string ConfirmAppointment(Guid appointmentId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { AppointmentId = appointmentId, IdempotencyKey = idempotencyKey });

    public static string StartAppointment(Guid appointmentId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { AppointmentId = appointmentId, IdempotencyKey = idempotencyKey });

    public static string CancelAppointment(Guid appointmentId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { AppointmentId = appointmentId, IdempotencyKey = idempotencyKey });

    public static string MarkNoShowAppointment(Guid appointmentId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { AppointmentId = appointmentId, IdempotencyKey = idempotencyKey });

    public static string CreateMedicalRecord(Guid appointmentId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { AppointmentId = appointmentId, IdempotencyKey = idempotencyKey });

    public static string UpdateAnamnesis(Guid medicalRecordId, string anamnesis, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { MedicalRecordId = medicalRecordId, Anamnesis = anamnesis, IdempotencyKey = idempotencyKey });

    public static string RecordVitalSigns(
        Guid medicalRecordId,
        decimal weightKg,
        decimal temperatureC,
        int? heartRateBpm,
        int? respiratoryRateBpm,
        string mucousMembranes,
        string capillaryRefillTime,
        DateTimeOffset measuredAt,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            MedicalRecordId = medicalRecordId,
            WeightKg = weightKg,
            TemperatureC = temperatureC,
            HeartRateBpm = heartRateBpm,
            RespiratoryRateBpm = respiratoryRateBpm,
            MucousMembranes = mucousMembranes,
            CapillaryRefillTime = capillaryRefillTime,
            MeasuredAt = measuredAt,
            IdempotencyKey = idempotencyKey
        });

    public static string AddEvolutionNote(Guid medicalRecordId, string text, Guid noteId, DateTimeOffset recordedAt, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            MedicalRecordId = medicalRecordId,
            Text = text,
            NoteId = noteId,
            RecordedAt = recordedAt,
            IdempotencyKey = idempotencyKey
        });

    public static string SetDiagnosis(Guid medicalRecordId, string diagnosis, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { MedicalRecordId = medicalRecordId, Diagnosis = diagnosis, IdempotencyKey = idempotencyKey });

    public static string SetConduct(Guid medicalRecordId, string conduct, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { MedicalRecordId = medicalRecordId, Conduct = conduct, IdempotencyKey = idempotencyKey });

    public static string FinalizeMedicalRecord(Guid medicalRecordId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { MedicalRecordId = medicalRecordId, IdempotencyKey = idempotencyKey });

    public static string RequestClinicalExam(Guid appointmentId, string name, string category, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { AppointmentId = appointmentId, Name = name, Category = category, IdempotencyKey = idempotencyKey });
}
