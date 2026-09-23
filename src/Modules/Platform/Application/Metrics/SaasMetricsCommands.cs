using Core.Domain;
using MediatR;

namespace Platform.Application.Metrics;

/// <summary>Upserts acquisition spend for CAC (10.2).</summary>
public sealed record UpsertAcquisitionSpendCommand(
    int Year,
    int Month,
    string Channel,
    decimal Amount,
    string? Note) : IRequest<Result>;
