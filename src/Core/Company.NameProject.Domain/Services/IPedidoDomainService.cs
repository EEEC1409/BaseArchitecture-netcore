using Company.NameProject.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Services
{
    public interface IPedidoDomainService
    {
        void ValidarPedido(Cliente cliente, Vendedor vendedor, List<PedidoDetalle> items);
    }
}
