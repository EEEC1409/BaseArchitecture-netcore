using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.ValueObjects;

namespace Company.NameProject.Domain.Entities
{
    public class PedidoDetalle
    {
        public Guid ProductoId { get; private set; }
        public int Cantidad { get; private set; }
        public decimal Precio { get; private set; }
        public Money Subtotal { get; private set; }

        private PedidoDetalle() { }

        public PedidoDetalle(Guid productoId, int cantidad, decimal precio, Money subtotal)
        {
            if (productoId == Guid.Empty)
                throw new DomainException("ProductoId inválido");
            if (cantidad <= 0)
                throw new DomainException("Cantidad inválida");
            if (precio < 0)
                throw new DomainException("Precio inválido");

            ProductoId = productoId;
            Cantidad = cantidad;
            Precio = precio;
            Subtotal = subtotal;
        }
    }
}

