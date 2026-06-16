using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.Entities.Events;
using Company.NameProject.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Entities
{
    public class Pedido : AggregateRoot
    {
        private readonly List<PedidoDetalle> _detalles = new();

        public Guid ClienteId { get; private set; }
        public Guid VendedorId { get; private set; }
        public Money Total { get; private set; } = Money.Crear(0, "USD");

        public IReadOnlyCollection<PedidoDetalle> Detalles => _detalles.AsReadOnly();

        private Pedido() { }

        public static Pedido Crear(Guid clienteId, Guid vendedorId)
        {
            if (clienteId == Guid.Empty)
                throw new DomainException("Cliente inválido");


            return new Pedido
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                VendedorId = vendedorId
            };
        }

        public void AgregarDetalle(Guid productoId, int cantidad, decimal precio)
        {
            if (cantidad <= 0)
                throw new DomainException("Cantidad inválida");
            var subtotal = Money.Crear(precio * cantidad, "USD");

            _detalles.Add(new PedidoDetalle(productoId, cantidad, precio, subtotal));
            Total = Total.Sumar(subtotal);
        }

        public void Confirmar()
        {
            AddEvent(new PedidoCreadoEvent(Id, Total.Amount));
        }
    }
}
