namespace Inventory.Application.Labels;

/// <summary>
/// Renders product label sheets as PDF bytes (Infrastructure implementation).
/// </summary>
public interface IProductLabelPdfRenderer
{
    /// <summary>
    /// Builds a multi-page PDF with one 60×40 mm label per copy.
    /// </summary>
    byte[] Render(IReadOnlyList<ProductLabelRenderModel> labels);
}

/// <summary>
/// Label content for PDF rendering.
/// </summary>
public sealed record ProductLabelRenderModel(string ProductName, string Sku, string Barcode, int Copies);
