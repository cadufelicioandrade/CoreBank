namespace CoreBank.Ledger.API.DTO
{
    public record StatementItemResponse(
    Guid Id,
    string Type,
    string Operation,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt);
}
