using Company.NameProject.Application.Common.Interfaces;
using Company.NameProject.Domain.Repositories;
using Company.NameProject.Infrastructure.Repositories;
using Company.NameProject.Infrastructure.Services;
using Company.NameProject.Persistence;
#if (IncludeRabbit)
using Company.NameProject.Infrastructure.Messaging;
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.NameProject.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Persistence layer
            services.AddPersistence(configuration);

            // Repositories
            services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
            services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();

            // Utilities
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

            // Health Checks
            var connectionString = configuration.GetConnectionString("DefaultConnection")!;
            services.AddHealthChecks()
#if (IsSQLServer)
                .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "ready"]);
#elif (IsPostgreSQL)
                .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "ready"]);
#else
                .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "ready"]);
#endif

            // Background services
            services.AddHostedService<OutboxProcessorService>();

#if (IncludeRabbit)
            // RabbitMQ Messaging
            services.Configure<RabbitMqSettings>(
                configuration.GetSection(RabbitMqSettings.SectionName));
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
#endif

            return services;
        }
    }
}


