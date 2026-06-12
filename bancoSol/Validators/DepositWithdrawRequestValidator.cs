using bancoSol.DTOs;
using FluentValidation;

namespace bancoSol.Validators
{
    public class DepositWithdrawRequestValidator : AbstractValidator<DepositWithdrawRequest>
    {
        public DepositWithdrawRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("El monto debe ser mayor a 0.");

            RuleFor(x => x.Description)
                .MaximumLength(255)
                .WithMessage("La descripción no puede tener más de 255 caracteres.");
        }
    }
}
