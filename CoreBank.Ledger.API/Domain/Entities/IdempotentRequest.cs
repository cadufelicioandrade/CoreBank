using System;

namespace CoreBank.Ledger.API.Domain.Entities
{
    /// <summary>
    /// Registra uma requisição já processada (idempotência).
    /// A ideia é: para cada RequestId + Endpoint, guardamos a TransactionId
    /// que foi criada anteriormente, para não processar duas vezes.
    /// </summary>
    public class IdempotentRequest
    {
        // RequestId que veio do cliente
        public Guid Id { get; private set; }

        // Qual endpoint foi chamado (ex: POST /api/ledger/transactions)
        public string Endpoint { get; private set; } = null!;

        // Id da transação criada da primeira vez que processamos esse RequestId
        public Guid TransactionId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        // Construtor para EF
        private IdempotentRequest() { }

        private IdempotentRequest(Guid id, string endpoint, Guid transactionId)
        {
            Id = id;
            Endpoint = endpoint;
            TransactionId = transactionId;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Fábrica estática para criar um registro de idempotência.
        /// (É essa Create que você está tentando usar no service.)
        /// </summary>
        public static IdempotentRequest Create(Guid id, string endpoint, Guid transactionId)
            => new IdempotentRequest(id, endpoint, transactionId);
    }
}
