using CoreBank.Ledger.API.Domain.Enums;

namespace CoreBank.Ledger.API.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

        public TransactionType Type { get; set; }
        public TransactionStatus Operation { get; set; }

        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }

        public string? Description { get; set; }

        public Guid? CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AccountNumber { get; internal set; }

        private Transaction() { }

        public Transaction(
            Guid accountId,
            TransactionType type,
            TransactionStatus operation,
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
