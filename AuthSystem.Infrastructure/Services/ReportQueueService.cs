using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.MessageQueue;
using AuthSystem.Domain.Models.MessageQueue.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de cola de generación de reportes
    /// </summary>
    public class ReportQueueService : IReportQueueService
    {
        private readonly IMessageBusService _messageBusService;
        private readonly MessageQueueSettings _settings;
        private readonly ILogger<ReportQueueService> _logger;

        public ReportQueueService(
            IMessageBusService messageBusService,
            IOptions<MessageQueueSettings> settings,
            ILogger<ReportQueueService> logger)
        {
            _messageBusService = messageBusService ?? throw new ArgumentNullException(nameof(messageBusService));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task QueueReportAsync(string reportType, Dictionary<string, object> parameters, string outputFormat = "PDF", 
            string notificationEmail = null, CancellationToken cancellationToken = default)
        {
            var reportMessage = new ReportMessage
            {
                ReportType = reportType,
                Parameters = parameters,
                OutputFormat = outputFormat,
                NotificationEmail = notificationEmail,
                NotifyOnCompletion = !string.IsNullOrEmpty(notificationEmail),
                CreatedAt = DateTime.UtcNow
            };

            await QueueReportAsync(reportMessage, cancellationToken);
        }

        /// <inheritdoc />
        public async Task QueueReportAsync(ReportMessage reportMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Encolando reporte de tipo {ReportType} en formato {OutputFormat}", 
                    reportMessage.ReportType, reportMessage.OutputFormat);

                // Si el reporte está programado para una fecha futura
                if (reportMessage.ScheduledFor.HasValue && reportMessage.ScheduledFor > DateTime.UtcNow)
                {
                    await _messageBusService.PublishScheduledAsync(reportMessage, reportMessage.ScheduledFor.Value, cancellationToken);
                    _logger.LogInformation("Reporte programado para {ScheduledTime}", reportMessage.ScheduledFor.Value);
                }
                // Caso normal: encolar en la cola específica de reportes
                else
                {
                    await _messageBusService.PublishToQueueAsync(reportMessage, _settings.QueueNames.ReportQueue, cancellationToken);
                    _logger.LogInformation("Reporte encolado exitosamente en {QueueName}", _settings.QueueNames.ReportQueue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar reporte de tipo {ReportType}: {ErrorMessage}", 
                    reportMessage.ReportType, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task QueueBulkReportAsync(IEnumerable<ReportMessage> reportMessages, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Encolando múltiples reportes");
                
                foreach (var reportMessage in reportMessages)
                {
                    await QueueReportAsync(reportMessage, cancellationToken);
                }
                
                _logger.LogInformation("Múltiples reportes encolados exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar múltiples reportes: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
