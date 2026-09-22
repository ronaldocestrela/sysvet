using Core.Domain;

namespace Commerce.Domain;

/// <summary>
/// Commerce module error codes for Result failures.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Product offer validation errors.</summary>
    public static class Offer
    {
        public static readonly Error NotFound = new("Commerce.Offer.NotFound", "Oferta não encontrada.");
        public static readonly Error DuplicateProduct = new("Commerce.Offer.DuplicateProduct", "Já existe oferta para este produto.");
        public static readonly Error InvalidPrice = new("Commerce.Offer.InvalidPrice", "Preço de venda deve ser maior que zero.");
        public static readonly Error NotPublished = new("Commerce.Offer.NotPublished", "Oferta não está publicada na loja.");
    }

    /// <summary>Online order validation errors.</summary>
    public static class Order
    {
        public static readonly Error NotFound = new("Commerce.Order.NotFound", "Pedido online não encontrado.");
        public static readonly Error Empty = new("Commerce.Order.Empty", "Pedido deve conter ao menos um item.");
        public static readonly Error InvalidTransition = new("Commerce.Order.InvalidTransition", "Transição de status inválida.");
        public static readonly Error BuyerRequired = new("Commerce.Order.BuyerRequired", "Nome e telefone do comprador são obrigatórios.");
        public static readonly Error InsufficientStock = new("Commerce.Order.InsufficientStock", "Estoque insuficiente para o pedido.");
    }

    /// <summary>Monetary value errors.</summary>
    public static class Money
    {
        public static readonly Error InvalidAmount = new("Commerce.Money.InvalidAmount", "Valor monetário inválido.");
    }

    /// <summary>Marketplace integration errors.</summary>
    public static class Marketplace
    {
        public static readonly Error NotConfigured = new("Commerce.Marketplace.NotConfigured", "Integração Mercado Livre não configurada.");
        public static readonly Error SellerNotMapped = new("Commerce.Marketplace.SellerNotMapped", "Vendedor Mercado Livre não mapeado ao tenant.");
    }
}
