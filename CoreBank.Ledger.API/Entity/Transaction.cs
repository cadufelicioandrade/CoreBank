using CoreBank.Ledger.API.Enums;

namespace CoreBank.Ledger.API.Entity
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

        public TransactionType Type { get; set; }
        public OperationType Operation { get; set; }

        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }

        public string? Description { get; set; }

        public Guid? CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }

        private Transaction() { }

        public Transaction(
            Guid accountId,
            TransactionType type,
            OperationType operation,
            decimal amount,
            decimal balanceAfter,
            string? description,
            Guid? correlationId)
        {
            AccountId = accountId;
            Type = type;
            Operation = operation;
            Amount = amount;
            BalanceAfter = balanceAfter;
            CorrelationId = correlationId;
        }

    }
}
