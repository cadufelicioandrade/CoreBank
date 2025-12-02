using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBank.Ledger.API.Domain.Events;

namespace CoreBank.Ledger.API.Application.Interfaces
{
    /// <summary>
    /// Contrato para a infraestrutura despachar eventos de domínio
    /// para handlers (mensageria, filas, etc).
    /// </summary>
    public interface IDomainEventsDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
    }
}
