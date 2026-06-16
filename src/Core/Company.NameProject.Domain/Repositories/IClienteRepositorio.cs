using Company.NameProject.Domain.Entities;

namespace Company.NameProject.Domain.Repositories
{
    public interface IClienteRepositorio : IGenericRepository<Cliente>
    {
        Task<bool> ExistePorNombre(string nombre);
        Task<Cliente?> GetByCedulaAsync(string cedula, CancellationToken cancellationToken = default);
    }
}

