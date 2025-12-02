using System;
using System.Collections.Generic;
using CoreBank.Ledger.API.Domain.Enums;
using CoreBank.Ledger.API.Domain.Events;

namespace CoreBank.Ledger.API.Domain.Entities
{
    public class LedgerTransaction
    {
        public Guid Id { get; private set; }
        public string AccountNumber { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Lista interna de eventos de domínio
        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// Eventos de domínio gerados por essa entidade.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private LedgerTransaction() { }

        private LedgerTransaction(string accountNumber, decimal amount, TransactionType type)
        {
            Id = Guid.NewGuid();
            AccountNumber = accountNumber;
            Amount = amount;
            Type = type;
            Status = TransactionStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public static LedgerTransaction Create(string accountNumber, decimal amount, TransactionType type)
        {
            return new LedgerTransaction(accountNumber, amount, type);
        }

        /// <summary>
        /// Adiciona um evento de domínio à entidade.
        /// </summary>
        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Limpa eventos (normalmente após despachá-los).
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        // Exemplo: método para marcar como concluída
        public void MarkAsCompleted()
        {
            Status = TransactionStatus.Completed;
        }
    }
}
