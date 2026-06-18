using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.Entities.Events;
using Company.NameProject.Persistence.Entities;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace Company.NameProject.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContext)
            : base(dbContext)
        {
        }

        // TODO: Agregar DbSet por cada entidad de negocio al implementarlas.
        // Ejemplo:
        // public DbSet<Cliente> Clientes { get; set; }
        // public DbSet<Vendedor> Vendedores { get; set; }
        // public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Configurar mapeos Fluent API por entidad al implementarlas.
            // Ejemplo:
            // modelBuilder.Entity<Cliente>(entity => { ... });

            // OutboxMessage
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("Outbox");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.OccurredOn).IsRequired();
            });
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var message = BuildDbUpdateMessage(ex);
                throw new Exception(message, ex);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var domainEvents = ChangeTracker.Entries<IHasDomainEvents>()
                    .SelectMany(e => e.Entity.DomainEvents)
                    .ToList();

                foreach (var domainEvent in domainEvents)
                {
                    Outbox.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        OccurredOn = DateTime.UtcNow,
                        Type = domainEvent.GetType().FullName!,
                        Content = JsonSerializer.Serialize(domainEvent),
                        Processed = false
                    });
                }

                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                var message = BuildDbUpdateMessage(ex);
                throw new Exception(message, ex);
            }
        }

        private string BuildDbUpdateMessage(DbUpdateException ex)
        {
            var message = $"Error al guardar cambios: {ex.InnerException?.Message ?? ex.Message}";

            if (ex.Entries.Any())
            {
                message += "\nEntidades afectadas:";
                foreach (var entry in ex.Entries)
                    message += $"\n- {entry.Entity.GetType().Name} [{entry.State}]";
            }

            return message;
        }
    }
}

