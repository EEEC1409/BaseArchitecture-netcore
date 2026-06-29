using Company.NameProject.Domain.Entities.Events;

using MediatR;

namespace Company.NameProject.Domain.Common
{
    /// <summary>
    /// Aggregate Root genérico. Soporta cualquier tipo de identificador (Guid, long, int).
    /// Implementa <see cref="IHasDomainEvents"/> para el despacho de eventos de dominio.
    /// </summary>
    public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    {
        private readonly List<INotification> _events = new();

        public IReadOnlyCollection<INotification> DomainEvents => _events;

        protected void AddEvent(INotification @event) => _events.Add(@event);

        public void ClearEvents() => _events.Clear();
    }

    /// <summary>
    /// Alias de <see cref="AggregateRoot{TId}"/> con <c>Guid</c> como identificador.
    /// Usar para entidades con alto volumen de registros o distribuidas.
    /// </summary>
    public abstract class AggregateRoot : AggregateRoot<Guid> { }

    /// <summary>
    /// Alias de <see cref="AggregateRoot{TId}"/> con <c>long</c> como identificador.
    /// Usar para entidades con volumen moderado donde se prefiere un entero de 64 bits.
    /// </summary>
    public abstract class AggregateRootLong : AggregateRoot<long> { }

    /// <summary>
    /// Alias de <see cref="AggregateRoot{TId}"/> con <c>int</c> como identificador.
    /// Usar para tablas pequeñas/catálogos sin millones de registros.
    /// </summary>
    public abstract class AggregateRootInt : AggregateRoot<int> { }
}
