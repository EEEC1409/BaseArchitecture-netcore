using Company.NameProject.Domain.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Crear(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("Monto inválido");

            return new Money(amount, currency);
        }

        public Money Sumar(Money otro)
        {
            if (Currency != otro.Currency)
                throw new DomainException("Moneda distinta");

            return new Money(Amount + otro.Amount, Currency);
        }
    }
}
