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
        public static readonly Error InvalidId = new("Order.InvalidId", "Identificador do pedido inválido.");
        public static readonly Error InvalidSeller = new("Order.InvalidSeller", "Vendedor inválido.");
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
        public static readonly Error InvalidDiscountPercent = new("Order.InvalidDiscountPercent", "Desconto deve estar entre 0 e 100.");
        public static readonly Error DiscountExceedsProfileLimit = new("Order.DiscountExceedsProfileLimit", "Desconto excede o limite do perfil de acesso.");
        public static readonly Error ReturnNotAllowed = new("Order.ReturnNotAllowed", "Devolução não permitida para o status atual do pedido.");
        public static readonly Error ReturnExceedsRemainingQuantity = new("Order.ReturnExceedsRemainingQuantity", "Quantidade devolvida excede o saldo da linha.");
        public static readonly Error ReturnEmpty = new("Order.ReturnEmpty", "Informe ao menos uma linha para devolução.");
        public static readonly Error ReturnItemNotFound = new("Order.ReturnItemNotFound", "Linha do pedido não encontrada.");
        public static readonly Error FinanceLinkNotAllowed = new("Order.FinanceLinkNotAllowed", "Integração financeira não está pendente para este pedido.");
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
        public static readonly Error NsuRequired = new("Payment.NsuRequired", "NSU é obrigatório para pagamentos eletrônicos.");
        public static readonly Error NsuNotAllowed = new("Payment.NsuNotAllowed", "Pagamento em dinheiro não deve informar NSU.");
        public static readonly Error RefundExceedsRemaining = new("Payment.RefundExceedsRemaining", "Valor do estorno excede o saldo do pagamento.");
        public static readonly Error RefundNotAllowed = new("Payment.RefundNotAllowed", "Estorno não permitido para o status atual do pedido.");
        public static readonly Error NotFound = new("Payment.NotFound", "Pagamento não encontrado.");
        public static readonly Error TerminalFailed = new("Payment.TerminalFailed", "Falha na comunicação com a maquininha.");
    }

    public static class CashRegister
    {
        public static readonly Error NotFound = new("CashRegister.NotFound", "Caixa não encontrado.");
        public static readonly Error AlreadyClosed = new("CashRegister.AlreadyClosed", "O caixa já está fechado.");
        public static readonly Error AlreadyOpen = new("CashRegister.AlreadyOpen", "O usuário já possui um caixa aberto.");
        public static readonly Error InvalidId = new("CashRegister.InvalidId", "Identificador do caixa inválido.");
    }

    public static class Money
    {
        public static readonly Error InvalidAmount = new("Money.InvalidAmount", "O valor não pode ser negativo.");
    }

    public static class CommissionRule
    {
        public static readonly Error InvalidId = new("CommissionRule.InvalidId", "Identificador da regra inválido.");
        public static readonly Error InvalidRate = new("CommissionRule.InvalidRate", "Percentual de comissão deve estar entre 0 e 100.");
        public static readonly Error NotFound = new("CommissionRule.NotFound", "Regra de comissão não encontrada.");
    }

    public static class Commission
    {
        public static readonly Error InvalidReverseAmount = new("Commission.InvalidReverseAmount", "Valor de estorno de comissão inválido.");
    }

    public static class Kit
    {
        public static readonly Error InvalidId = new("Kit.InvalidId", "Identificador do kit inválido.");
        public static readonly Error NameRequired = new("Kit.NameRequired", "Nome do kit é obrigatório.");
        public static readonly Error EmptyComponents = new("Kit.EmptyComponents", "O kit deve conter ao menos um produto.");
        public static readonly Error UnknownOffer = new("Kit.UnknownOffer", "Kit não encontrado ou inativo.");
        public static readonly Error InvalidComponent = new("Kit.InvalidComponent", "Componente do kit inválido.");
        public static readonly Error InvalidComponentQuantity = new("Kit.InvalidComponentQuantity", "Quantidade do componente deve ser maior que zero.");
    }

    public static class Package
    {
        public static readonly Error InvalidId = new("Package.InvalidId", "Identificador do pacote inválido.");
        public static readonly Error NameRequired = new("Package.NameRequired", "Nome do pacote é obrigatório.");
        public static readonly Error InvalidUsesPerUnit = new("Package.InvalidUsesPerUnit", "Usos por unidade deve ser maior que zero.");
        public static readonly Error UnknownOffer = new("Package.UnknownOffer", "Pacote não encontrado ou inativo.");
        public static readonly Error PetRequired = new("Package.PetRequired", "Pacote pré-pago exige tutor e pet no pedido.");
        public static readonly Error InsufficientBalance = new("Package.InsufficientBalance", "Saldo de usos insuficiente.");
        public static readonly Error ServiceMismatch = new("Package.ServiceMismatch", "Serviço não corresponde ao saldo do pacote.");
        public static readonly Error PetMismatch = new("Package.PetMismatch", "Pet não corresponde ao saldo do pacote.");
        public static readonly Error ReturnAfterConsumption = new("Package.ReturnAfterConsumption", "Não é possível devolver usos já consumidos do pacote.");
        public static readonly Error InvalidUses = new("Package.InvalidUses", "Quantidade de usos inválida.");
        public static readonly Error InvalidUsageId = new("Package.InvalidUsageId", "Identificador de consumo inválido.");
        public static readonly Error NotFound = new("Package.NotFound", "Saldo pré-pago não encontrado.");
    }
}
