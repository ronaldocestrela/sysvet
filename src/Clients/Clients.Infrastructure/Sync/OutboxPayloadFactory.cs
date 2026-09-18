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

    public static string CreatePet(Guid id, string name, string species, string breed, string sex, Guid tutorId, DateOnly? birthDate, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            Sex = sex,
            TutorId = tutorId,
            BirthDate = birthDate,
            IdempotencyKey = idempotencyKey
        });

    public static string UpdatePet(Guid id, string name, string species, string breed, string sex, DateOnly? birthDate, DateTimeOffset occurredAt, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            Name = name,
            Species = species,
            Breed = breed,
            Sex = sex,
            BirthDate = birthDate,
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

    public static string CreateClinicalQuote(Guid appointmentId, string? notes, Guid quoteId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            AppointmentId = appointmentId,
            Notes = notes,
            QuoteId = quoteId,
            IdempotencyKey = idempotencyKey
        });

    public static string ReplaceClinicalQuoteItems(
        Guid quoteId,
        IReadOnlyList<Crm.ClinicalQuoteLineDto> items,
        string? notes,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            QuoteId = quoteId,
            Items = items.Select(i => new
            {
                i.Id,
                i.Description,
                i.Quantity,
                i.UnitPrice,
                i.Kind,
                ProductId = (Guid?)null,
                i.SortOrder
            }),
            Notes = notes,
            IdempotencyKey = idempotencyKey
        });

    public static string SendClinicalQuote(Guid quoteId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { QuoteId = quoteId, IdempotencyKey = idempotencyKey });

    public static string ApproveClinicalQuote(Guid quoteId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { QuoteId = quoteId, IdempotencyKey = idempotencyKey });

    public static string RejectClinicalQuote(Guid quoteId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { QuoteId = quoteId, IdempotencyKey = idempotencyKey });

    public static string RegisterVaccineDose(
        Guid id,
        Guid petId,
        string name,
        string batchNumber,
        DateTimeOffset appliedAt,
        DateTimeOffset? nextDueDate,
        Guid? protocolDoseId,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Id = id,
            PetId = petId,
            Name = name,
            BatchNumber = batchNumber,
            AppliedAt = appliedAt,
            NextDueDate = nextDueDate,
            ProtocolDoseId = protocolDoseId,
            IdempotencyKey = idempotencyKey
        });

    public static string AdmitPet(Guid petId, Guid veterinarianId, Guid bedId, string reason, Guid hospitalizationId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            PetId = petId,
            VeterinarianId = veterinarianId,
            BedId = bedId,
            Reason = reason,
            HospitalizationId = hospitalizationId,
            IdempotencyKey = idempotencyKey
        });

    public static string DischargePet(Guid hospitalizationId, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new { HospitalizationId = hospitalizationId, IdempotencyKey = idempotencyKey });

    public static string AdministerMedication(Guid hospitalizationId, Guid administrationId, string? notes, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            HospitalizationId = hospitalizationId,
            AdministrationId = administrationId,
            Notes = notes,
            IdempotencyKey = idempotencyKey
        });

    public static string SkipMedication(Guid hospitalizationId, Guid administrationId, string? notes, Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            HospitalizationId = hospitalizationId,
            AdministrationId = administrationId,
            Notes = notes,
            IdempotencyKey = idempotencyKey
        });

    public static string CreateMedicationOrder(
        Guid hospitalizationId,
        string medicationName,
        string dose,
        string route,
        IReadOnlyList<TimeOnly> dailyTimes,
        DateOnly startsOn,
        DateOnly endsOn,
        Guid orderId,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            HospitalizationId = hospitalizationId,
            MedicationName = medicationName,
            Dose = dose,
            Route = route,
            DailyTimes = dailyTimes,
            StartsOn = startsOn,
            EndsOn = endsOn,
            OrderId = orderId,
            IdempotencyKey = idempotencyKey
        });

    public static string RegisterProduct(
        Guid productId,
        string name,
        string description,
        string sku,
        string barcode,
        string unitOfMeasure,
        decimal reorderLevel,
        Inventory.Domain.Enums.ProductCategory category,
        string ncm,
        string? cest,
        int merchandiseOrigin,
        Guid? supplierId,
        bool? requiresLot,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            Name = name,
            Description = description,
            Sku = sku,
            Barcode = barcode,
            UnitOfMeasure = unitOfMeasure,
            ReorderLevel = reorderLevel,
            Category = category,
            Ncm = ncm,
            Cest = cest,
            MerchandiseOrigin = merchandiseOrigin,
            SupplierId = supplierId,
            RequiresLot = requiresLot,
            ProductId = productId,
            IdempotencyKey = idempotencyKey
        });

    public static string RegisterProductLot(
        Guid productId,
        string lotNumber,
        DateTimeOffset? expirationDate,
        decimal unitCost,
        decimal initialQuantity,
        Guid lotId,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            ProductId = productId,
            LotNumber = lotNumber,
            ExpirationDate = expirationDate,
            UnitCost = unitCost,
            InitialQuantity = initialQuantity,
            LotId = lotId,
            IdempotencyKey = idempotencyKey
        });

    public static string RegisterSupplier(
        Guid supplierId,
        string legalName,
        string tradeName,
        string document,
        string? contactEmail,
        string? contactPhone,
        Guid idempotencyKey) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            LegalName = legalName,
            TradeName = tradeName,
            Document = document,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            SupplierId = supplierId,
            IdempotencyKey = idempotencyKey
        });
}
