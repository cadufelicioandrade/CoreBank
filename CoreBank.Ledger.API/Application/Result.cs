namespace CoreBank.Ledger.API.Application
{
    /// <summary>
    /// Resultado sem payload, só indica sucesso/erro.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }

        protected Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        // ==========================
        // Versões SEM payload
        // ==========================

        public static Result Success()
            => new Result(true, null);
        
        public static Result Fail(string error)
            => new Result(false, error);

        public static Result Ok()
            => Success();

        // ==========================
        // Versões COM payload (T)
        // ==========================

        // Sucesso com retorno de valor
        public static Result<T> Success<T>(T value)
            => new Result<T>(true, value, null);

        // Falha com retorno de valor
        public static Result<T> Fail<T>(string error)
            => new Result<T>(false, default, error);

        // Alias mais “natural” para sucesso com payload
        public static Result<T> Ok<T>(T value)
            => Success(value);
    }

    /// <summary>
    /// Resultado com payload de tipo T.
    /// </summary>
    public class Result<T> : Result
    {
        public T? Value { get; }

        internal Result(bool isSuccess, T? value, string? error)
            : base(isSuccess, error)
        {
            Value = value;
        }
    }
}
