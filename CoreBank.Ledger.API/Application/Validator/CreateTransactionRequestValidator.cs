using CoreBank.Ledger.API.Application.DTO;
using FluentValidation;

namespace CoreBank.Ledger.API.Application.Validator
{
    // ==================================================
    // 5) Validação com FluentValidation
    // ==================================================
    // Valida regras básicas do CreateTransactionRequest antes
    // de executar a lógica de negócio.
    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
    {
        public CreateTransactionRequestValidator()
        {
            // RequestId não pode ser vazio (Guid.Empty)
            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("RequestId é obrigatório para idempotência.");

            // Número da conta obrigatório
            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("AccountNumber é obrigatório.")
                .MaximumLength(50).WithMessage("AccountNumber excede o tamanho máximo permitido.");

            // Valor deve ser maior que zero
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount deve ser maior que zero.");

            // Tipo deve ser CREDIT ou DEBIT
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type é obrigatório.")
                .Must(t =>
                {
                    var upper = t.ToUpperInvariant();
                    return upper == "CREDIT" || upper == "DEBIT";
                })
                .WithMessage("Type deve ser CREDIT ou DEBIT.");
        }
    }
}
