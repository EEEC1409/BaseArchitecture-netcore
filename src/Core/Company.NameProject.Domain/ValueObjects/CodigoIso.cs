using Company.NameProject.Domain.Common;

namespace Company.NameProject.Domain.ValueObjects
{
    public sealed class CodigoIso : IEquatable<CodigoIso>
    {
        public string Value { get; }

        private CodigoIso(string value) => Value = value;

        public static CodigoIso Crear(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Código ISO requerido");

            if (value.Trim().Length != 2)
                throw new DomainException("Código ISO inválido — debe tener exactamente 2 caracteres");

            return new CodigoIso(value.Trim().ToUpperInvariant());
        }

        public bool Equals(CodigoIso? other) => other is not null && Value == other.Value;

        public override bool Equals(object? obj) => obj is CodigoIso other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

        public override string ToString() => Value;

        public static bool operator ==(CodigoIso? left, CodigoIso? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(CodigoIso? left, CodigoIso? right) => !(left == right);
    }
}
