using MediatR;

namespace Company.NameProject.Domain.Common
{
   

    public abstract class AggregateRoot : Entity<Guid>
    {
        private readonly List<INotification> _events = new();

        public IReadOnlyCollection<INotification> Events => _events;

        protected void AddEvent(INotification @event)
        {
            _events.Add(@event);
        }

        public void ClearEvents() => _events.Clear();
    }
}
