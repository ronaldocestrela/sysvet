using Core.Domain;

namespace Finance.Domain;

/// <summary>
/// Standardized error codes for the Finance module.
/// </summary>
public static class ErrorCodes
{
    public static class Money
    {
        public static readonly Error InvalidAmount = new("Finance.Money.InvalidAmount", "O valor não pode ser negativo.");
    }

    public static class Title
    {
        public static readonly Error NotFound = new("FinancialTitle.NotFound", "Título financeiro não encontrado.");
        public static readonly Error InvalidAmount = new("FinancialTitle.InvalidAmount", "Valor do título deve ser maior que zero.");
        public static readonly Error InvalidParty = new("FinancialTitle.InvalidParty", "Contraparte inválida para o título.");
        public static readonly Error InvalidDueDate = new("FinancialTitle.InvalidDueDate", "Data de vencimento é obrigatória.");
        public static readonly Error CancelNotAllowed = new("FinancialTitle.CancelNotAllowed", "Cancelamento não permitido para este título.");
        public static readonly Error SettleNotAllowed = new("FinancialTitle.SettleNotAllowed", "Baixa não permitida para o status atual.");
        public static readonly Error AllocationExceedsOpen = new("FinancialTitle.AllocationExceedsOpen", "Valor da baixa excede o saldo em aberto.");
        public static readonly Error DuplicateReversal = new("FinancialTitle.DuplicateReversal", "Estorno já registrado para esta referência.");
        public static readonly Error InvalidDescription = new("FinancialTitle.InvalidDescription", "Descrição é obrigatória.");
    }

    public static class Category
    {
        public static readonly Error NotFound = new("FinancialCategory.NotFound", "Categoria financeira não encontrada.");
        public static readonly Error InvalidCode = new("FinancialCategory.InvalidCode", "Código da categoria é obrigatório.");
        public static readonly Error InvalidName = new("FinancialCategory.InvalidName", "Nome da categoria é obrigatório.");
        public static readonly Error SystemImmutable = new("FinancialCategory.SystemImmutable", "Categorias de sistema não podem ser alteradas.");
    }

    public static class CostCenter
    {
        public static readonly Error NotFound = new("CostCenter.NotFound", "Centro de custo não encontrado.");
        public static readonly Error InvalidCode = new("CostCenter.InvalidCode", "Código do centro de custo é obrigatório.");
        public static readonly Error InvalidName = new("CostCenter.InvalidName", "Nome do centro de custo é obrigatório.");
    }

    public static class Reconciliation
    {
        public static readonly Error NotFound = new("CardReconciliation.NotFound", "Lote de conciliação não encontrado.");
        public static readonly Error InvalidReference = new("CardReconciliation.InvalidReference", "Referência do lote é obrigatória.");
        public static readonly Error EmptyStatement = new("CardReconciliation.EmptyStatement", "Informe ao menos uma linha no extrato.");
        public static readonly Error InvalidLine = new("CardReconciliation.InvalidLine", "Linha do extrato inválida (NSU e valor obrigatórios).");
    }
}
