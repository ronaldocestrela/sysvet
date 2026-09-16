using Core.Domain;

namespace Sales.Domain;

/// <summary>
/// Standardized error codes for the Sales module.
/// </summary>
public static class ErrorCodes
{
    public static class Order
    {
        public static readonly Error NotFound = new("Order.NotFound", "Pedido não encontrado.");
        public static readonly Error CashRegisterNotOpen = new("Order.CashRegisterNotOpen", "O caixa informado não existe ou não está aberto.");
        public static readonly Error NotDraft = new("Order.NotDraft", "Não é possível adicionar itens a um pedido que não está em rascunho.");
        public static readonly Error InvalidQuantity = new("Order.InvalidQuantity", "A quantidade deve ser maior que zero.");
        public static readonly Error InvalidStatus = new("Order.InvalidStatus", "Apenas pedidos em rascunho ou aguardando pagamento podem ser pagos.");
        public static readonly Error Empty = new("Order.Empty", "Não é possível pagar um pedido sem itens.");
    }

    public static class CashRegister
    {
        public static readonly Error NotFound = new("CashRegister.NotFound", "Caixa não encontrado.");
        public static readonly Error AlreadyClosed = new("CashRegister.AlreadyClosed", "O caixa já está fechado.");
        public static readonly Error AlreadyOpen = new("CashRegister.AlreadyOpen", "O usuário já possui um caixa aberto.");
    }

    public static class Money
    {
        public static readonly Error InvalidAmount = new("Money.InvalidAmount", "O valor não pode ser negativo.");
    }
}
