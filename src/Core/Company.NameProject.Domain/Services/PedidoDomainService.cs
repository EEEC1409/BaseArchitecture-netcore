using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Services
{
    public class PedidoDomainService : IPedidoDomainService
    {
        public void ValidarPedido(Cliente cliente, Vendedor vendedor, List<PedidoDetalle> items)
        {
            if (!cliente.Activo)
                throw new DomainException("Cliente inactivo");

            if (items.Count == 0)
                throw new DomainException("El pedido debe tener items");

            if (items.Sum(x => x.Subtotal.Amount) <= 0)
                throw new DomainException("Total inválido");
        }
    }
}
