namespace CoreBank.Ledger.API.DTO
{
    public record CreateTransactionRequest(
    Guid AccountId,
    string Type,       // "credit" ou "debit"
    string Operation,  // "deposit", "withdraw", "pix_in", "pix_out"
    decimal Amount,
    string? Description,
    Guid? CorrelationId);
}
