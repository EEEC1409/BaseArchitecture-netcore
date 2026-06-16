using FluentValidation;

namespace Company.NameProject.Application.CQRS.Commands.Clientes.Crear
{
    public class CrearClienteValidator : AbstractValidator<CrearClienteCommand>
    {
        public CrearClienteValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

            RuleFor(x => x.Identificacion)
                .NotEmpty().WithMessage("La identificación es requerida.")
                .MinimumLength(10).WithMessage("La identificación debe tener al menos 10 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.");
        }
    }
}

