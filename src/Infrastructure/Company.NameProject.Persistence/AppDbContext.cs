using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Configurar mapeos Fluent API por entidad al implementarlas.
            // Ejemplo:
            // modelBuilder.Entity<Cliente>(entity => { ... });
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

