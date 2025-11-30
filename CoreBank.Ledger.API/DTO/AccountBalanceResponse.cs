namespace CoreBank.Ledger.API.DTO
{
    public record AccountBalanceResponse(
    Guid AccountId,
    decimal CurrentBalance,
    DateTime UpdatedAt);
}
