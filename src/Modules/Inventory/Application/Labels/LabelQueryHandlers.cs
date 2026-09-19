using System.Text;
using Core.Domain;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Labels;

/// <summary>Builds PDF or ZPL label files for catalog products.</summary>
public sealed class GenerateProductLabelsQueryHandler : IRequestHandler<GenerateProductLabelsQuery, Result<LabelFileDto>>
{
    private const int MaxProducts = 50;
    private const int MaxCopies = 50;

    private readonly IProductRepository _productRepository;
    private readonly IProductLabelPdfRenderer _pdfRenderer;

    public GenerateProductLabelsQueryHandler(IProductRepository productRepository, IProductLabelPdfRenderer pdfRenderer)
    {
        _productRepository = productRepository;
        _pdfRenderer = pdfRenderer;
    }

    public async Task<Result<LabelFileDto>> Handle(GenerateProductLabelsQuery request, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return Result.Failure<LabelFileDto>(Inventory.Domain.ErrorCodes.ProductLabel.EmptyRequest);
        }

        if (request.Items.Count > MaxProducts)
        {
            return Result.Failure<LabelFileDto>(Inventory.Domain.ErrorCodes.ProductLabel.TooManyItems);
        }

        var renderModels = new List<ProductLabelRenderModel>();
        foreach (var item in request.Items)
        {
            if (item.Copies is < 1 or > MaxCopies)
            {
                return Result.Failure<LabelFileDto>(Inventory.Domain.ErrorCodes.ProductLabel.InvalidCopies);
            }

            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null || !product.IsActive)
            {
                return Result.Failure<LabelFileDto>(Inventory.Domain.ErrorCodes.Product.NotFound);
            }

            renderModels.Add(new ProductLabelRenderModel(product.Name, product.Sku, product.Barcode, item.Copies));
        }

        return request.Format switch
        {
            LabelFormat.Zpl => Result.Success(BuildZpl(renderModels)),
            LabelFormat.Pdf => Result.Success(BuildPdf(renderModels)),
            _ => Result.Failure<LabelFileDto>(Inventory.Domain.ErrorCodes.ProductLabel.UnsupportedFormat)
        };
    }

    private static LabelFileDto BuildZpl(IReadOnlyList<ProductLabelRenderModel> models)
    {
        var sb = new StringBuilder();
        foreach (var model in models)
        {
            sb.Append(Domain.Services.ZplLabelEncoder.Encode(model.ProductName, model.Sku, model.Barcode, model.Copies));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return new LabelFileDto(bytes, "text/plain", "product-labels.zpl");
    }

    private LabelFileDto BuildPdf(IReadOnlyList<ProductLabelRenderModel> models)
    {
        var bytes = _pdfRenderer.Render(models);
        return new LabelFileDto(bytes, "application/pdf", "product-labels.pdf");
    }
}
