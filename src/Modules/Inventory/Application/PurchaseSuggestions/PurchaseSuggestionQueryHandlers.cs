using System.Globalization;
using System.Text;
using Core.Domain;
using Inventory.Application.Labels;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.PurchaseSuggestions;

/// <summary>Lists purchase suggestions grouped by supplier.</summary>
public sealed class ListPurchaseSuggestionsQueryHandler : IRequestHandler<ListPurchaseSuggestionsQuery, Result<IReadOnlyList<PurchaseSuggestionGroupDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly ISupplierRepository _supplierRepository;

    public ListPurchaseSuggestionsQueryHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        ISupplierRepository supplierRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<IReadOnlyList<PurchaseSuggestionGroupDto>>> Handle(ListPurchaseSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var groups = await PurchaseSuggestionProjection.BuildAsync(
            _productRepository,
            _lotRepository,
            _supplierRepository,
            request.SupplierId,
            cancellationToken);
        return Result.Success(groups);
    }
}

/// <summary>Exports purchase suggestions as UTF-8 CSV.</summary>
public sealed class ExportPurchaseSuggestionsQueryHandler : IRequestHandler<ExportPurchaseSuggestionsQuery, Result<LabelFileDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly ISupplierRepository _supplierRepository;

    public ExportPurchaseSuggestionsQueryHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        ISupplierRepository supplierRepository)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<LabelFileDto>> Handle(ExportPurchaseSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var groups = await PurchaseSuggestionProjection.BuildAsync(
            _productRepository,
            _lotRepository,
            _supplierRepository,
            request.SupplierId,
            cancellationToken);

        var csv = BuildCsv(groups);
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
        return Result.Success(new LabelFileDto(bytes, "text/csv", "purchase-suggestions.csv"));
    }

    private static string BuildCsv(IReadOnlyList<PurchaseSuggestionGroupDto> groups)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Fornecedor;CNPJ;Produto;SKU;CodigoBarras;Saldo;PontoPedido;EstoqueAlvo;QtdSugerida;CustoMedio;TotalEstimado");
        foreach (var group in groups)
        {
            foreach (var line in group.Lines)
            {
                sb.Append(Csv(group.SupplierName)).Append(';');
                sb.Append(Csv(group.SupplierDocument ?? "")).Append(';');
                sb.Append(Csv(line.ProductName)).Append(';');
                sb.Append(Csv(line.Sku)).Append(';');
                sb.Append(Csv(line.Barcode)).Append(';');
                sb.Append(Format(line.OnHand)).Append(';');
                sb.Append(Format(line.ReorderLevel)).Append(';');
                sb.Append(Format(line.TargetStock)).Append(';');
                sb.Append(Format(line.SuggestedQuantity)).Append(';');
                sb.Append(Format(line.AverageCost)).Append(';');
                sb.AppendLine(Format(line.EstimatedTotal));
            }
        }

        return sb.ToString();
    }

    private static string Format(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Csv(string value) =>
        value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
