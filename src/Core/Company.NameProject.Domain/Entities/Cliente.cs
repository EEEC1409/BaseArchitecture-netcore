using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Entities
{
    public class Cliente : AggregateRoot
    {
        public string Cedula { get; private set; }
        public string Nombre { get; private set; }
        public bool Activo { get; private set; }
        public Email Email { get; private set; }

        private Cliente() { }

        public static Cliente Crear(string nombre, string cedula, string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new DomainException("Nombre requerido");

            if (string.IsNullOrWhiteSpace(cedula))
                throw new DomainException("Cédula requerido");

            return new Cliente
            {
                Id = Guid.NewGuid(),
                Cedula = cedula,
                Nombre = nombre,
                Activo = true,
                Email = Email.Crear(email)
            };
        }
    }
}
