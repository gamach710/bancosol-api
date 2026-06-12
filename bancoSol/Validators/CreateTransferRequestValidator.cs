using bancoSol.DTOs;
using FluentValidation;

namespace bancoSol.Validators
{
    public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
    {
        public CreateTransferRequestValidator()
        {
            RuleFor(x => x.SourceAccountNumber)
                .NotEmpty().WithMessage("La cuenta origen es requerida.");

            RuleFor(x => x.DestinationAccountNumber)
                .NotEmpty().WithMessage("La cuenta destino es requerida.")
                .NotEqual(x => x.SourceAccountNumber)
                .WithMessage("La cuenta origen y destino no pueden ser la misma.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty().WithMessage("La clave de idempotencia es requerida.");

            RuleFor(x => x.Description)
                .MaximumLength(255).WithMessage("La descripción no puede tener más de 255 caracteres.");
        }
    }
}
