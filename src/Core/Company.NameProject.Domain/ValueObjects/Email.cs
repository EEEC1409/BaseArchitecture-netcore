using Company.NameProject.Domain.Common;

namespace Company.NameProject.Domain.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        public string Value { get; }

        private Email(string value) => Value = value;

        public static Email Crear(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Email requerido");

            if (!value.Contains('@'))
                throw new DomainException("Email inválido");

            return new Email(value.Trim().ToLowerInvariant());
        }

        public bool Equals(Email? other) => other is not null && Value == other.Value;

        public override bool Equals(object? obj) => obj is Email other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Value;

        public static bool operator ==(Email? left, Email? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(Email? left, Email? right) => !(left == right);
    }
}
