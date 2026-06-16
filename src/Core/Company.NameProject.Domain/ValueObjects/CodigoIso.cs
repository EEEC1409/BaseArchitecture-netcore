using Company.NameProject.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.ValueObjects
{
    public class CodigoIso
    {
        public string Value { get; }

        private CodigoIso(string value)
        {
            Value = value;
        }

        public static CodigoIso Crear(string value)
        {
            if (value.Length != 2)
                throw new DomainException("Código ISO inválido");

            return new CodigoIso(value.ToUpper());
        }
    }
}
