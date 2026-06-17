using Company.NameProject.Application.CQRS.Commands.Clientes.Crear;
using Company.NameProject.Application.CQRS.Queries.Cliente;
using Company.NameProject.Shared.Common;
using Company.NameProject.Shared.Exceptions;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.NameProject.WebApi
{
    [Authorize]
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearClienteCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(ApiResponse<Guid>.Success(id, "Cliente creado correctamente"));
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var resultado = await _mediator.Send(new ListarClientesQuery(page, pageSize));
            return Ok(ApiResponse<PagedResult<ClienteDto>>.Success(resultado));
        }

        [HttpGet("{cedula}")]
        public async Task<IActionResult> ObtenerPorCedula(string cedula)
        {
            var cliente = await _mediator.Send(new ObtenerClientePorCedulaQuery(cedula));
            return Ok(ApiResponse<ClienteDto>.Success(cliente));
        }
    }
}

