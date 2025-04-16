using AuthSystem.Domain.Models.MessageQueue.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de cola de correos electrónicos
    /// </summary>
    public interface IEmailQueueService
    {
        /// <summary>
        /// Encola un correo electrónico para su envío asíncrono
        /// </summary>
        /// <param name="to">Destinatario del correo</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task QueueEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Encola un correo electrónico completo para su envío asíncrono
        /// </summary>
        /// <param name="emailMessage">Mensaje de correo completo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task QueueEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Encola múltiples correos electrónicos para su envío asíncrono
        /// </summary>
        /// <param name="emailMessages">Lista de mensajes de correo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task QueueBulkEmailAsync(IEnumerable<EmailMessage> emailMessages, CancellationToken cancellationToken = default);
    }
}
