using bancoSol.DTOs;
using FluentValidation;

namespace bancoSol.Validators
{
    public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
    {
        private static readonly string[] SupportedCurrencies = { "BOB", "USD" };

        public CreateAccountValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El primer nombre es requerido.")
                .MaximumLength(150).WithMessage("El primer nombre no puede tener más de 150 caracteres.");

            RuleFor(x => x.FirstLastName)
                .NotEmpty().WithMessage("El primer apellido es requerido.")
                .MaximumLength(150).WithMessage("El primer apellido no puede tener más de 150 caracteres.");

            RuleFor(x => x.CI)
                .NotEmpty().WithMessage("El CI es requerido.")
                .MaximumLength(20).WithMessage("El CI no puede tener más de 20 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("La moneda es requerida.")
                .Must(c => SupportedCurrencies.Contains(c?.ToUpper()))
                .WithMessage("Moneda '{PropertyValue}' no soportada. Las monedas válidas son: BOB, USD.");

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
        }
    }
}