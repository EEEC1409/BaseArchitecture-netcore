using Company.NameProject.Application.Common.Behaviors;
using Company.NameProject.Application.CQRS.Commands.Clientes.Crear;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Company.NameProject.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(CrearClienteHandler).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
