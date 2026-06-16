using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.Entities;
using Company.NameProject.Domain.Entities.Events;
using Company.NameProject.Domain.ValueObjects;
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

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Cedula).IsRequired().HasMaxLength(20);
                entity.OwnsOne(c => c.Email, email =>
                {
                    email.Property(e => e.Value)
                         .HasColumnName("Email")
                         .IsRequired()
                         .HasMaxLength(256);
                });
            });

            // Vendedor
            modelBuilder.Entity<Vendedor>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Nombre).IsRequired().HasMaxLength(200);
            });

            // Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.OwnsOne(p => p.Total, money =>
                {
                    money.Property(m => m.Amount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
                });
                entity.OwnsMany(p => p.Detalles, detalle =>
                {
                    detalle.WithOwner().HasForeignKey("PedidoId");
                    detalle.Property<Guid>("Id").ValueGeneratedOnAdd();
                    detalle.HasKey("Id");
                    detalle.Property(d => d.Cantidad).IsRequired();
                    detalle.Property(d => d.Precio).HasColumnType("decimal(18,2)");
                    detalle.Property(d => d.ProductoId).IsRequired();
                    detalle.OwnsOne(d => d.Subtotal, money =>
                    {
                        money.Property(m => m.Amount).HasColumnName("SubtotalAmount").HasColumnType("decimal(18,2)");
                        money.Property(m => m.Currency).HasColumnName("SubtotalCurrency").HasMaxLength(3);
                    });
                });
            });

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

