namespace CoreBank.Ledger.API.Domain.Enums
{
    public enum TransactionStatus : byte
    {
        Pending = 1,
        Confirmed = 2,
        Rejected = 3,
        Completed = 4
    }
}
