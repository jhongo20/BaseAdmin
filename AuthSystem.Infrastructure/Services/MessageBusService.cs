using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.MessageQueue;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de bus de mensajes utilizando MassTransit y RabbitMQ
    /// </summary>
    public class MessageBusService : IMessageBusService
    {
        private readonly IBus _bus;
        private readonly MessageQueueSettings _settings;
        private readonly ILogger<MessageBusService> _logger;

        public MessageBusService(IBus bus, IOptions<MessageQueueSettings> settings, ILogger<MessageBusService> logger)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                _logger.LogInformation("Publicando mensaje de tipo {MessageType}", typeof(T).Name);
                await _bus.Publish(message, cancellationToken);
                _logger.LogInformation("Mensaje de tipo {MessageType} publicado exitosamente", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al publicar mensaje de tipo {MessageType}: {ErrorMessage}", 
                    typeof(T).Name, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task PublishToQueueAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                _logger.LogInformation("Publicando mensaje de tipo {MessageType} a la cola {QueueName}", 
                    typeof(T).Name, queueName);
                
                var endpoint = await _bus.GetSendEndpoint(new Uri($"queue:{queueName}"));
                await endpoint.Send(message, cancellationToken);
                
                _logger.LogInformation("Mensaje de tipo {MessageType} enviado exitosamente a la cola {QueueName}", 
                    typeof(T).Name, queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar mensaje de tipo {MessageType} a la cola {QueueName}: {ErrorMessage}", 
                    typeof(T).Name, queueName, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task PublishScheduledAsync<T>(T message, DateTime scheduledTime, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                _logger.LogInformation("Programando mensaje de tipo {MessageType} para {ScheduledTime}", 
                    typeof(T).Name, scheduledTime);
                
                // Obtener el programador de mensajes
                var scheduler = _bus.GetSendEndpoint(new Uri("queue:scheduler")).Result;
                
                // Calcular el tiempo de espera
                var delay = scheduledTime - DateTime.UtcNow;
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }
                
                // Enviar el mensaje programado
                await scheduler.Send(message, context => context.Delay = delay, cancellationToken);
                
                _logger.LogInformation("Mensaje de tipo {MessageType} programado exitosamente para {ScheduledTime}", 
                    typeof(T).Name, scheduledTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al programar mensaje de tipo {MessageType} para {ScheduledTime}: {ErrorMessage}", 
                    typeof(T).Name, scheduledTime, ex.Message);
                throw;
            }
        }
    }
}
