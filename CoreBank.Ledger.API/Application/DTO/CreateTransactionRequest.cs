namespace CoreBank.Ledger.API.Application.DTO
{
    public sealed class CreateTransactionRequest
    {
        /// <summary>
        /// Id único da requisição (para idempotência). Deve vir do client.
        /// </summary>
        public Guid RequestId { get; set; }

        /// <summary>
        /// Número da conta a ser debitada/creditada.
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Valor numérico da transação.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Tipo de transação: "CREDIT" ou "DEBIT".
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}
