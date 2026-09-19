using Inventory.Application.Labels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace Inventory.Infrastructure.Labels;

/// <summary>
/// Renders product labels to PDF using QuestPDF (Community license) and ZXing Code128.
/// </summary>
public sealed class QuestPdfProductLabelRenderer : IProductLabelPdfRenderer
{
    static QuestPdfProductLabelRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public byte[] Render(IReadOnlyList<ProductLabelRenderModel> labels)
    {
        var document = Document.Create(container =>
        {
            foreach (var label in labels)
            {
                for (var copy = 0; copy < label.Copies; copy++)
                {
                    container.Page(page =>
                    {
                        page.Size(60, 40, Unit.Millimetre);
                        page.Margin(2, Unit.Millimetre);
                        page.Content().Column(column =>
                        {
                            column.Spacing(2);
                            column.Item().Text(label.ProductName).FontSize(9).Bold();
                            column.Item().Text($"SKU: {label.Sku}").FontSize(8);
                            var barcodeBytes = RenderBarcodePng(label.Barcode);
                            column.Item().Height(12, Unit.Millimetre).Image(barcodeBytes);
                            column.Item().Text(label.Barcode).FontSize(7);
                        });
                    });
                }
            }
        });

        return document.GeneratePdf();
    }

    private static byte[] RenderBarcodePng(string barcode)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Height = 80,
                Width = 240,
                Margin = 2,
                PureBarcode = false
            }
        };

        using var bitmap = writer.Write(barcode);
        using var image = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return image.ToArray();
    }
}
