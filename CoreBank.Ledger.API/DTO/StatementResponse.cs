namespace CoreBank.Ledger.API.DTO
{
    public record StatementResponse(
    Guid AccountId,
    decimal CurrentBalance,
    DateTime UpdatedAt,
    int Page,
    int PageSize,
    int TotalItems,
    IEnumerable<StatementItemResponse> Items);
}
