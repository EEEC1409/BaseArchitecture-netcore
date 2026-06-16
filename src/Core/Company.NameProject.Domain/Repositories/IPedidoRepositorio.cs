using Company.NameProject.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Repositories
{
    public interface IPedidoRepositorio : IGenericRepository<Pedido>
    {
        Task<bool> ExistePedidoActivo(Guid clienteId);
        Task<List<Pedido>> ObtenerPorCliente(Guid clienteId);
    }
}
