namespace Core.Application.Privacy;

/// <summary>
/// Tombstone and original identifiers captured at anonymization time.
/// </summary>
/// <param name="TutorId">CRM tutor aggregate id.</param>
/// <param name="TombstoneName">Replacement display name.</param>
/// <param name="TombstoneEmail">Replacement e-mail.</param>
/// <param name="TombstoneCpf">Replacement CPF digits.</param>
/// <param name="TombstonePhone">Replacement phone digits.</param>
/// <param name="OriginalCpf">Pre-erasure CPF used to match fiscal copies.</param>
/// <param name="OriginalEmail">Pre-erasure e-mail.</param>
public sealed record PersonalDataErasureContext(
    Guid TutorId,
    string TombstoneName,
    string TombstoneEmail,
    string TombstoneCpf,
    string TombstonePhone,
    string OriginalCpf,
    string OriginalEmail);
