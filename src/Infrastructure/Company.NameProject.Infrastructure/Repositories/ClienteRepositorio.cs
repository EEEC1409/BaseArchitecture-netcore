using Company.NameProject.Domain.Entities;
using Company.NameProject.Domain.Repositories;
using Company.NameProject.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Company.NameProject.Infrastructure.Repositories
{
    public class ClienteRepositorio : GenericRepository<Cliente>, IClienteRepositorio
    {
        public ClienteRepositorio(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistePorNombre(string nombre)
            => await _context.Clientes.AnyAsync(x => x.Nombre == nombre);

        public async Task<Cliente?> GetByCedulaAsync(string cedula, CancellationToken cancellationToken = default)
            => await _context.Clientes.FirstOrDefaultAsync(x => x.Cedula == cedula, cancellationToken);
    }
}

