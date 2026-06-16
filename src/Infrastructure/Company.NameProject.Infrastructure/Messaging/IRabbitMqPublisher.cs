namespace Company.NameProject.Infrastructure.Messaging
{
    /// <summary>
    /// Contrato para publicar mensajes/eventos en RabbitMQ.
    /// </summary>
    public interface IRabbitMqPublisher
    {
        /// <summary>Publica un mensaje en el exchange configurado.</summary>
        Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
            where T : class;
    }
}
