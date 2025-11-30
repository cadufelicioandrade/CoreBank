namespace CoreBank.Ledger.API.Entity
{
    public class AccountBalance
    {
        public Guid AccountId { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime UpdatedAt { get; set; }

        //controle de concorrência otimista
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        private AccountBalance(){}

        public AccountBalance(Guid accountId)
        {
            AccountId = accountId;
            CurrentBalance = 0m;
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal ApplyTransaction(decimal amount, bool isCredit)
        {
            var delta = isCredit ? amount : -amount;
            CurrentBalance += delta;
            UpdatedAt = DateTime.UtcNow;
            return CurrentBalance;
        }
    }
}
