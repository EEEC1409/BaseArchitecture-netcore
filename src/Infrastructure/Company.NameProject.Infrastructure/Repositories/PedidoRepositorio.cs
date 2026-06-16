using Company.NameProject.Domain.Entities;
using Company.NameProject.Domain.Repositories;
using Company.NameProject.Persistence;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Infrastructure.Repositories
{
    public class PedidoRepositorio : GenericRepository<Pedido>, IPedidoRepositorio
    {
        public PedidoRepositorio(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistePedidoActivo(Guid clienteId)
        {
            return await _context.Pedidos
                .AnyAsync(p => p.ClienteId == clienteId);
        }

        public async Task<List<Pedido>> ObtenerPorCliente(Guid clienteId)
        {
            return await _context.Pedidos
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }
    }
}
