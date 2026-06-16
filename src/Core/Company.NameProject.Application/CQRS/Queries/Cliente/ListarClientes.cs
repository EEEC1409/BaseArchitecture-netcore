using Company.NameProject.Domain.Repositories;

using MediatR;

namespace Company.NameProject.Application.CQRS.Queries.Cliente
{
    public record ListarClientesQuery() : IRequest<List<ClienteDto>>;

    public class ListarClientesHandler : IRequestHandler<ListarClientesQuery, List<ClienteDto>>
    {
        private readonly IClienteRepositorio _repo;

        public ListarClientesHandler(IClienteRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<List<ClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
        {
            var clientes = await _repo.GetAllAsync(cancellationToken);

            return clientes
                .Select(c => new ClienteDto(c.Id, c.Nombre, c.Cedula, c.Email.Value, c.Activo))
                .ToList();
        }
    }
}

