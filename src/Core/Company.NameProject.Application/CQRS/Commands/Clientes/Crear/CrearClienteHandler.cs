using Company.NameProject.Domain.Entities;
using Company.NameProject.Domain.Repositories;

using MediatR;

namespace Company.NameProject.Application.CQRS.Commands.Clientes.Crear
{
    public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, Guid>
    {
        private readonly IClienteRepositorio _clienteRepo;

        public CrearClienteHandler(IClienteRepositorio clienteRepo)
        {
            _clienteRepo = clienteRepo;
        }

        public async Task<Guid> Handle(CrearClienteCommand request, CancellationToken ct)
        {
            var cliente = Cliente.Crear(request.Nombre, request.Identificacion, request.Email);
            await _clienteRepo.AddAsync(cliente, ct);
            return cliente.Id;
        }
    }
}

