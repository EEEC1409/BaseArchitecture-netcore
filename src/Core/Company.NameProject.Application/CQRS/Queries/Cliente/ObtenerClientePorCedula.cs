using Company.NameProject.Domain.Repositories;
using Company.NameProject.Shared.Exceptions;

using FluentValidation;

using MediatR;

namespace Company.NameProject.Application.CQRS.Queries.Cliente
{
    public record ObtenerClientePorCedulaQuery(string Cedula) : IRequest<ClienteDto>;

    public class ObtenerClientePorCedulaValidator : AbstractValidator<ObtenerClientePorCedulaQuery>
    {
        public ObtenerClientePorCedulaValidator()
        {
            RuleFor(x => x.Cedula)
                .NotEmpty().WithMessage("La cédula es requerida.")
                .MaximumLength(20).WithMessage("La cédula no puede superar 20 caracteres.");
        }
    }

    public class ObtenerClientePorCedulaHandler : IRequestHandler<ObtenerClientePorCedulaQuery, ClienteDto>
    {
        private readonly IClienteRepositorio _repo;

        public ObtenerClientePorCedulaHandler(IClienteRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<ClienteDto> Handle(ObtenerClientePorCedulaQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _repo.GetByCedulaAsync(request.Cedula, cancellationToken);

            if (cliente is null)
                throw ApiException.NotFound($"Cliente con cédula '{request.Cedula}'");

            return new ClienteDto(cliente.Id, cliente.Nombre, cliente.Cedula, cliente.Email.Value, cliente.Activo);
        }
    }
}
