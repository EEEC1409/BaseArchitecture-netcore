using Company.NameProject.Domain.Repositories;
using Company.NameProject.Shared.Common;

using MediatR;

namespace Company.NameProject.Application.CQRS.Queries.Cliente
{
    public record ListarClientesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<ClienteDto>>;

    public class ListarClientesHandler : IRequestHandler<ListarClientesQuery, PagedResult<ClienteDto>>
    {
        private readonly IClienteRepositorio _repo;

        public ListarClientesHandler(IClienteRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<ClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
        {
            var paginado = await _repo.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

            var dtos = paginado.Items
                .Select(c => new ClienteDto(c.Id, c.Nombre, c.Cedula, c.Email.Value, c.Activo))
                .ToList();

            return new PagedResult<ClienteDto>(dtos, paginado.TotalCount, paginado.Page, paginado.PageSize);
        }
    }
}

