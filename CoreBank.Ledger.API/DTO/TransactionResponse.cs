namespace CoreBank.Ledger.API.DTO
{
    public record TransactionResponse(
    Guid Id,
    Guid AccountId,
    string Type,
    string Operation,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    Guid? CorrelationId,
    DateTime CreatedAt);
}
