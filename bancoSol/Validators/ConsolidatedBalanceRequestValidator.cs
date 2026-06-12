using bancoSol.DTOs;
using FluentValidation;

namespace bancoSol.Validators
{
    public class ConsolidatedBalanceRequestValidator
        : AbstractValidator<ConsolidatedBalanceRequest>
    {
        public ConsolidatedBalanceRequestValidator()
        {
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("La moneda es requerida.")
                .Must(c => c == "BOB" || c == "USD")
                .WithMessage("Moneda '{PropertyValue}' no soportada. Use BOB o USD.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("La fecha de inicio es requerida.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("La fecha de fin es requerida.")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("La fecha de fin debe ser mayor o igual a la fecha de inicio.");
        }
    }
}