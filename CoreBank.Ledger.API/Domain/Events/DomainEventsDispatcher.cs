using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBank.Ledger.API.Application.Interfaces;
using CoreBank.Ledger.API.Domain.Events;
using Microsoft.Extensions.Logging;

namespace CoreBank.Ledger.API.Infrastructure
{
    /// <summary>
    /// Implementação simplificada que só loga os eventos.
    /// Em um projeto real, aqui você publicaria em Rabbit, Kafka, etc.
    /// </summary>
    public class DomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly ILogger<DomainEventsDispatcher> _logger;

        public DomainEventsDispatcher(ILogger<DomainEventsDispatcher> logger)
        {
            _logger = logger;
        }

        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
        {
            foreach (var ev in domainEvents)
            {
                _logger.LogInformation("Domain event dispatched: {EventType} at {OccurredAt}",
                    ev.GetType().Name, ev.OccurredAt);
            }

            return Task.CompletedTask;
        }
    }
}
