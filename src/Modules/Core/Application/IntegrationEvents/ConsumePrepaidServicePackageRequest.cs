using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Synchronous prepaid use consumption (handled by Sales; Petshop 6.6 and PDV API).
/// </summary>
public sealed class ConsumePrepaidServicePackageRequest : IRequest<Result>
{
    public Guid UsageId { get; }
    public Guid PetId { get; }
    public string ServiceCode { get; }
    public string? AttendanceRef { get; }

    public ConsumePrepaidServicePackageRequest(Guid usageId, Guid petId, string serviceCode, string? attendanceRef = null)
    {
        UsageId = usageId;
        PetId = petId;
        ServiceCode = serviceCode;
        AttendanceRef = attendanceRef;
    }
}
