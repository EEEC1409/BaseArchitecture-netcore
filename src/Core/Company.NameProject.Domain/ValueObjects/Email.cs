using Company.NameProject.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Crear(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Email requerido");

            if (!value.Contains("@"))
                throw new DomainException("Email inválido");

            return new Email(value);
        }

        public override bool Equals(object obj)
        {
            return obj is Email email &&
                   Value == email.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
