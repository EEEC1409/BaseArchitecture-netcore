using Company.NameProject.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Entities
{
    public class Vendedor : AggregateRoot
    {
        public string Nombre { get; private set; }

        public static Vendedor Crear(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new DomainException("Nombre inválido");

            return new Vendedor
            {
                Id = Guid.NewGuid(),
                Nombre = nombre
            };
        }
    }
}
