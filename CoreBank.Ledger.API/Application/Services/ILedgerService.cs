using CoreBank.Ledger.API.Application.DTO;

namespace CoreBank.Ledger.API.Application.Services
{
    public interface ILedgerService
    {
        Task<Result<TransactionResponse>> CreateAsync(CreateTransactionRequest request, CancellationToken ct = default);
        Task<Result<TransactionResponse?>> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Result<IEnumerable<TransactionResponse>>> GetByAccountAsync(string accountNumber, CancellationToken ct = default);
    }
}
