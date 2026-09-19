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
        public static readonly Error InvalidCashRegister = new("Order.InvalidCashRegister", "Caixa inválido.");
        public static readonly Error PetRequiresTutor = new("Order.PetRequiresTutor", "Informe o tutor ao vincular um pet.");
        public static readonly Error NotDraft = new("Order.NotDraft", "Não é possível adicionar itens a um pedido que não está em rascunho.");
        public static readonly Error InvalidQuantity = new("Order.InvalidQuantity", "A quantidade deve ser maior que zero.");
        public static readonly Error InvalidStatus = new("Order.InvalidStatus", "Apenas pedidos em rascunho ou aguardando pagamento podem ser pagos.");
        public static readonly Error Empty = new("Order.Empty", "Não é possível pagar um pedido sem itens.");
        public static readonly Error PaymentTotalMismatch = new("Order.PaymentTotalMismatch", "A soma dos pagamentos deve ser igual ao total do pedido.");
        public static readonly Error InsufficientStock = new("Order.InsufficientStock", "Estoque insuficiente para concluir a venda.");
        public static readonly Error TutorNotFound = new("Order.TutorNotFound", "Tutor não encontrado.");
        public static readonly Error PetNotFound = new("Order.PetNotFound", "Pet não encontrado.");
        public static readonly Error PetTutorMismatch = new("Order.PetTutorMismatch", "O pet não pertence ao tutor informado.");
    }

    public static class OrderItem
    {
        public static readonly Error ProductIdRequired = new("OrderItem.ProductIdRequired", "Produto inválido.");
        public static readonly Error DescriptionRequired = new("OrderItem.DescriptionRequired", "Descrição do serviço é obrigatória.");
    }

    public static class Payment
    {
        public static readonly Error Required = new("Payment.Required", "Informe ao menos uma forma de pagamento.");
        public static readonly Error ZeroAmount = new("Payment.ZeroAmount", "Valor do pagamento deve ser maior que zero.");
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
