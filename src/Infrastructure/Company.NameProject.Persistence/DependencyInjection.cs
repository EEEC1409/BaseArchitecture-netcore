using Company.NameProject.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.NameProject.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")!;

            services.AddDbContext<AppDbContext>(options =>
#if (IsSQLServer)
                options.UseSqlServer(connectionString));
#elif (IsPostgreSQL)
                options.UseNpgsql(connectionString));
#else
                options.UseSqlServer(connectionString));
#endif

            services.AddScoped<DomainEventDispatcher>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}


