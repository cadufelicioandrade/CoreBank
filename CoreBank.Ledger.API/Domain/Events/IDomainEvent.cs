using System;

namespace CoreBank.Ledger.API.Domain.Events
{
    /// <summary>
    /// Contrato base para todos os eventos de domínio.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
