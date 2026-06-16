using Company.NameProject.Persistence;
using Company.NameProject.Persistence.Entities;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace Company.NameProject.Infrastructure.Services
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        public OutboxProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensajes del Outbox.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var messages = await db.Outbox
                .Where(m => !m.Processed)
                .OrderBy(m => m.OccurredOn)
                .Take(50)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
                return;

            _logger.LogInformation("Procesando {Count} mensajes del Outbox.", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    var eventType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.FullName == message.Type || t.Name == message.Type);

                    if (eventType is not null)
                    {
                        var domainEvent = JsonSerializer.Deserialize(message.Content, eventType);
                        if (domainEvent is INotification notification)
                            await mediator.Publish(notification, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Tipo de evento no encontrado: {Type}", message.Type);
                    }

                    message.Processed = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensaje {Id} de tipo {Type}.", message.Id, message.Type);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
