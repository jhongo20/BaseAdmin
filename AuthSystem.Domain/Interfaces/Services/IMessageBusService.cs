using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de bus de mensajes que maneja la publicación de mensajes a colas
    /// </summary>
    public interface IMessageBusService
    {
        /// <summary>
        /// Publica un mensaje en la cola correspondiente
        /// </summary>
        /// <typeparam name="T">Tipo del mensaje</typeparam>
        /// <param name="message">Mensaje a publicar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publica un mensaje en una cola específica
        /// </summary>
        /// <typeparam name="T">Tipo del mensaje</typeparam>
        /// <param name="message">Mensaje a publicar</param>
        /// <param name="queueName">Nombre de la cola</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task PublishToQueueAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publica un mensaje programado para ser entregado en un momento específico
        /// </summary>
        /// <typeparam name="T">Tipo del mensaje</typeparam>
        /// <param name="message">Mensaje a publicar</param>
        /// <param name="scheduledTime">Momento programado para la entrega</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task PublishScheduledAsync<T>(T message, DateTime scheduledTime, CancellationToken cancellationToken = default) where T : class;
    }
}
