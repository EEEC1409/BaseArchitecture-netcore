using Company.NameProject.Domain.Common;

namespace Company.NameProject.Domain.ValueObjects
{
    public sealed class Money : IEquatable<Money>
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

            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException("Moneda requerida");

            return new Money(amount, currency.ToUpperInvariant());
        }

        public Money Sumar(Money otro)
        {
            if (Currency != otro.Currency)
                throw new DomainException("Moneda distinta");

            return new Money(Amount + otro.Amount, Currency);
        }

        public bool Equals(Money? other) =>
            other is not null && Amount == other.Amount && Currency == other.Currency;

        public override bool Equals(object? obj) => obj is Money other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Amount, Currency);

        public override string ToString() => $"{Amount} {Currency}";

        public static bool operator ==(Money? left, Money? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(Money? left, Money? right) => !(left == right);
    }
}
