namespace Inventory.Application.Labels;

/// <summary>
/// Supported label download formats.
/// </summary>
public enum LabelFormat
{
    Pdf,
    Zpl
}

/// <summary>
/// Binary label file returned by generate queries.
/// </summary>
public sealed record LabelFileDto(byte[] Content, string ContentType, string FileName);
