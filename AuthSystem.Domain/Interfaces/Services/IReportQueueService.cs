using AuthSystem.Domain.Models.MessageQueue.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de cola de generación de reportes
    /// </summary>
    public interface IReportQueueService
    {
        /// <summary>
        /// Encola un reporte para su generación asíncrona
        /// </summary>
        /// <param name="reportType">Tipo de reporte</param>
        /// <param name="parameters">Parámetros del reporte</param>
        /// <param name="outputFormat">Formato de salida (PDF, Excel, etc.)</param>
        /// <param name="notificationEmail">Email para notificar cuando el reporte esté listo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task QueueReportAsync(string reportType, Dictionary<string, object> parameters, string outputFormat = "PDF", 
            string notificationEmail = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Encola un reporte completo para su generación asíncrona
        /// </summary>
        /// <param name="reportMessage">Mensaje de reporte completo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task QueueReportAsync(ReportMessage reportMessage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Encola múltiples reportes para su generación asíncrona
        /// </summary>
        /// <param name="reportMessages">Lista de mensajes de reporte</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task QueueBulkReportAsync(IEnumerable<ReportMessage> reportMessages, CancellationToken cancellationToken = default);
    }
}
