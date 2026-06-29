using MediatR;

namespace Company.NameProject.Domain.Entities.Events
{
    /// <summary>
    /// Contrato que deben implementar todos los Aggregate Roots para exponer
    /// sus domain events, independientemente del tipo de identificador (Guid, long, int).
    /// </summary>
    public interface IHasDomainEvents
    {
        IReadOnlyCollection<INotification> DomainEvents { get; }
        void ClearEvents();
    }
}
