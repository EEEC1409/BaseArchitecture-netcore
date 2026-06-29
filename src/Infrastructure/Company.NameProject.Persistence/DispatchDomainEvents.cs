using Company.NameProject.Application.Common.Interfaces;
using Company.NameProject.Domain.Entities.Events;
using Company.NameProject.Persistence.Entities;

using System.Text.Json;

namespace Company.NameProject.Persistence
{
    public class DomainEventDispatcher
    {
        private readonly AppDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public DomainEventDispatcher(AppDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public Task DispatchAsync()
        {
            // Usa IHasDomainEvents en lugar de AggregateRoot para capturar
            // entidades con cualquier tipo de ID (Guid, long, int)
            var entities = _context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            foreach (var entity in entities)
            {
                foreach (var domainEvent in entity.DomainEvents)
                {
                    _context.Outbox.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                        Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                        OccurredOn = _dateTime.UtcNow,
                        Processed = false
                    });
                }

                entity.ClearEvents();
            }

            return Task.CompletedTask;
        }
    }
}
