using System;

namespace CoreBank.Ledger.API.Domain.Events
{
    /// <summary>
    /// Implementação base para eventos de domínio.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
    }
}
