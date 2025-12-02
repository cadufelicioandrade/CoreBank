using System;
using CoreBank.Ledger.API.Domain.Enums;

namespace CoreBank.Ledger.API.Domain.Events
{
    /// <summary>
    /// Evento de domínio disparado quando uma transação é criada.
    /// </summary>
    public class TransactionCreatedDomainEvent : DomainEvent
    {
        public Guid TransactionId { get; }
        public string AccountNumber { get; }
        public decimal Amount { get; }
        public TransactionType Type { get; }

        public TransactionCreatedDomainEvent(
            Guid transactionId,
            string accountNumber,
            decimal amount,
            TransactionType type)
        {
            TransactionId = transactionId;
            AccountNumber = accountNumber;
            Amount = amount;
            Type = type;
        }
    }
}
