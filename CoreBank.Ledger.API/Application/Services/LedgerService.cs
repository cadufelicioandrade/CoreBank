using CoreBank.Ledger.API.Application.DTO;
using CoreBank.Ledger.API.Application.Interfaces;
using CoreBank.Ledger.API.Domain.Entities;
using CoreBank.Ledger.API.Domain.Enums;
using CoreBank.Ledger.API.Domain.Events;
using CoreBank.Ledger.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CoreBank.Ledger.API.Application.Services
{
    /// <summary>
    /// Orquestra regras de criação e consulta de transações de ledger.
    /// Implementa:
    /// - Validação básica
    /// - Idempotência
    /// - Transações EF explícitas
    /// - Eventos de domínio pós-commit
    /// - Result Pattern (sucesso/erro)
    /// </summary>
    public class LedgerService : ILedgerService
    {
        private const string CreateTransactionEndpoint = "POST /api/ledger/transactions";

        private readonly LedgerDbContext _db;
        private readonly IDomainEventsDispatcher _domainEventsDispatcher;
        private readonly ILogger<LedgerService> _logger;

        public LedgerService(
            LedgerDbContext db,
            IDomainEventsDispatcher domainEventsDispatcher,
            ILogger<LedgerService> logger)
        {
            _db = db;
            _domainEventsDispatcher = domainEventsDispatcher;
            _logger = logger;
        }

        public async Task<Result<TransactionResponse>> CreateAsync(
            CreateTransactionRequest request,
            CancellationToken cancellation = default)
        {
            if (request.Amount <= 0)
                return Result.Fail<TransactionResponse>("Amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                return Result.Fail<TransactionResponse>("AccountNumber is required.");

            var type = request.Type.ToUpperInvariant() switch
            {
                "CREDIT" => TransactionType.Credit,
                "DEBIT" => TransactionType.Debit,
                _ => default(TransactionType?)
            };

            if (type is null)
                return Result.Fail<TransactionResponse>("Invalid transaction type. Use CREDIT or DEBIT.");

            // Idempotência: verifica se já existe RequestId + Endpoint (ex: usuário clicou várias vezes no botão de enviar)
            var existingIdempotent = await _db.IdempotentRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.RequestId &&
                    x.Endpoint == CreateTransactionEndpoint, cancellation);

            if (existingIdempotent is not null)
            {
                // Se já existe, busca a transação associada 
                var existingTransaction = await _db.Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == existingIdempotent.TransactionId, cancellation);

                if (existingTransaction is null)
                {
                    // Cenário raro de inconsistência
                    _logger.LogWarning(
                        "IdempotentRequest found but transaction {TransactionId} does not exist.",
                        existingIdempotent.TransactionId);

                    return Result.Fail<TransactionResponse>(
                        "Inconsistent idempotent state. Please contact support.");
                }

                _logger.LogInformation(
                    "Idempotent request detected. Returning existing transaction {TransactionId}",
                    existingTransaction.Id);

                return Result.Ok(MapToResponse(existingTransaction));
            }

            //Cria a entidade LedgerTransaction de transação em memória
            var transaction = LedgerTransaction.Create(
                request.AccountNumber,
                request.Amount,
                type.Value);

            // Por que marcar como COMPLETED?
            //Porque, no modelo atual do seu ledger, toda transação criada já nasce concluída.
            //Ela não passa por etapas intermediárias como:
            //PENDING,IN_REVIEW,FAILED,AUTHORIZED / CAPTURED
            //EVOLUIR ISSO MAIS PRA FRENTE!
            transaction.MarkAsCompleted();

            //Adiciona evento de domínio para ser disparado depois
            var domainEvent = new TransactionCreatedDomainEvent(
                transaction.Id,
                transaction.AccountNumber,
                transaction.Amount,
                transaction.Type);

            transaction.AddDomainEvent(domainEvent);

            //BeginTransaction explícito do EF
            await using var dbTransaction = await _db.Database.BeginTransactionAsync(cancellation);

            try
            {
                _db.Transactions.Add(transaction);

                var idem = IdempotentRequest.Create(
                    request.RequestId,
                    CreateTransactionEndpoint,
                    transaction.Id);

                _db.IdempotentRequests.Add(idem);

                await _db.SaveChangesAsync(cancellation);
                await dbTransaction.CommitAsync(cancellation);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync(cancellation);
                _logger.LogError(ex, "Error while creating ledger transaction.");
                return Result.Fail<TransactionResponse>("An unexpected error occurred.");
            }

            // Dispara eventos de domínio (pós-commit), para mensageria, avisar outro serviço, Kafka, Redis, email, saldo, fraude…
            await _domainEventsDispatcher.DispatchAsync(transaction.DomainEvents, cancellation);
            transaction.ClearDomainEvents();

            // 9) Loga sucesso
            _logger.LogInformation(
                "Transaction created. RequestId={RequestId}, Account={Account}, Amount={Amount}, Type={Type}, TransactionId={TransactionId}",
                request.RequestId,
                transaction.AccountNumber,
                transaction.Amount,
                transaction.Type,
                transaction.Id);

            return Result.Ok(MapToResponse(transaction));
        }

        public async Task<Result<TransactionResponse?>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var transaction = await _db.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            return Result.Ok(transaction is null ? null : MapToResponse(transaction));
        }

        public async Task<Result<IEnumerable<TransactionResponse>>> GetByAccountAsync(
            string accountNumber,
            CancellationToken ct = default)
        {
            var list = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.AccountNumber == accountNumber)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);

            return Result.Ok(list.Select(MapToResponse));
        }

        private static TransactionResponse MapToResponse(LedgerTransaction t)
            => new TransactionResponse
            {
                Id = t.Id,
                AccountNumber = t.AccountNumber,
                Amount = t.Amount,
                Type = t.Type.ToString().ToUpperInvariant(),
                Status = t.Status.ToString().ToUpperInvariant(),
                CreatedAt = t.CreatedAt
            };
    }
}
