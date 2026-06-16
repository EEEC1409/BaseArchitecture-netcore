using MediatR;

namespace Company.NameProject.Domain.Entities.Events
{
    public interface IHasDomainEvents
    {
        List<INotification> DomainEvents { get; }
    }
}
