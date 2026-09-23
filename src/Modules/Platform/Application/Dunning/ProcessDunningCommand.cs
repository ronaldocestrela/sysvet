using Core.Application.Messaging;

namespace Platform.Application.Dunning;

/// <summary>Runs dunning notices, card retries and operational lock evaluation (9.5).</summary>
/// <param name="AsOfUtc">Reference instant.</param>
public sealed record ProcessDunningCommand(DateTimeOffset AsOfUtc) : ICommand<int>;
