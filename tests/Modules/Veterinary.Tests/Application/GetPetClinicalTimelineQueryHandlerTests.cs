using FluentAssertions;
using NSubstitute;
using Veterinary.Application.MedicalRecords.Queries;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Tests.Application;

public class GetPetClinicalTimelineQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithRecords_ReturnsTimelineItems()
    {
        var petId = Guid.NewGuid();
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), petId).Value;
        record.SetAnamnesis("Cough");

        var medicalRecordRepository = Substitute.For<IMedicalRecordRepository>();
        medicalRecordRepository.GetByPetIdAsync(petId, Arg.Any<CancellationToken>()).Returns(new List<MedicalRecord> { record });

        var appointmentRepository = Substitute.For<IAppointmentRepository>();
        appointmentRepository.GetByIdAsync(record.AppointmentId, Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var handler = new GetPetClinicalTimelineQueryHandler(medicalRecordRepository, appointmentRepository);
        var result = await handler.Handle(new GetPetClinicalTimelineQuery(petId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(i => i.Id == record.Id);
    }
}
