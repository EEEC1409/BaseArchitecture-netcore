using MediatR;

namespace Company.NameProject.Application.CQRS.Commands.Clientes.Crear
{
    public record CrearClienteCommand(
        string Nombre,
        string Identificacion,
        string Email
    ) : IRequest<Guid>;
}

