using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.MessageQueue.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.MessageQueue.Consumers
{
    /// <summary>
    /// Consumidor de mensajes de generación de reportes
    /// </summary>
    public class ReportMessageConsumer : IConsumer<ReportMessage>
    {
        private readonly IReportService _reportService;
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<ReportMessageConsumer> _logger;

        public ReportMessageConsumer(
            IReportService reportService,
            IEmailQueueService emailQueueService,
            ILogger<ReportMessageConsumer> logger)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _emailQueueService = emailQueueService ?? throw new ArgumentNullException(nameof(emailQueueService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Procesa un mensaje de generación de reporte
        /// </summary>
        public async Task Consume(ConsumeContext<ReportMessage> context)
        {
            var message = context.Message;
            
            try
            {
                _logger.LogInformation("Procesando mensaje de generación de reporte {MessageId} de tipo {ReportType}", 
                    message.Id, message.ReportType);

                // Generar el reporte
                var reportResult = await _reportService.GenerateReportAsync(
                    message.ReportType,
                    message.Parameters,
                    message.OutputFormat);

                // Guardar el reporte en disco si se especificó una ruta
                string outputPath = message.OutputPath;
                if (string.IsNullOrEmpty(outputPath))
                {
                    // Crear una ruta por defecto si no se especificó
                    string fileName = $"{message.ReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.{message.OutputFormat.ToLower()}";
                    outputPath = Path.Combine(Path.GetTempPath(), "Reports", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                }

                // Guardar el reporte
                await File.WriteAllBytesAsync(outputPath, reportResult.ReportData);

                _logger.LogInformation("Reporte {MessageId} generado exitosamente y guardado en {OutputPath}", 
                    message.Id, outputPath);

                // Enviar notificación por correo electrónico si se solicitó
                if (message.NotifyOnCompletion && !string.IsNullOrEmpty(message.NotificationEmail))
                {
                    await SendNotificationEmail(message, outputPath);
                }

                // Si hay una URL de callback, notificar que el reporte está listo
                if (!string.IsNullOrEmpty(message.CallbackUrl))
                {
                    // Aquí se implementaría la lógica para notificar a través de la URL de callback
                    _logger.LogInformation("Notificando a través de callback URL: {CallbackUrl}", message.CallbackUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar mensaje de generación de reporte {MessageId}: {ErrorMessage}", 
                    message.Id, ex.Message);
                
                // Re-encolar el mensaje para reintento si es necesario
                if (context.GetRetryCount() < 3)
                {
                    _logger.LogWarning("Reintentando generación de reporte {MessageId}, intento {RetryCount}", 
                        message.Id, context.GetRetryCount() + 1);
                    throw; // MassTransit reintentará automáticamente
                }
                else
                {
                    _logger.LogError("Se alcanzó el número máximo de reintentos para el reporte {MessageId}", 
                        message.Id);
                    
                    // Notificar el fallo si se solicitó notificación
                    if (message.NotifyOnCompletion && !string.IsNullOrEmpty(message.NotificationEmail))
                    {
                        await SendFailureNotificationEmail(message, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Envía un correo electrónico de notificación cuando el reporte está listo
        /// </summary>
        private async Task SendNotificationEmail(ReportMessage message, string reportPath)
        {
            try
            {
                var emailMessage = new EmailMessage
                {
                    To = message.NotificationEmail,
                    Subject = $"Reporte {message.ReportType} generado exitosamente",
                    Body = $@"<html>
<body>
    <h2>Reporte Generado Exitosamente</h2>
    <p>El reporte solicitado ha sido generado correctamente.</p>
    <p><strong>Tipo de Reporte:</strong> {message.ReportType}</p>
    <p><strong>Formato:</strong> {message.OutputFormat}</p>
    <p><strong>Fecha de Generación:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    <p>El reporte se encuentra adjunto a este correo.</p>
</body>
</html>",
                    IsHtml = true,
                    SendImmediately = true
                };

                // Adjuntar el reporte
                if (File.Exists(reportPath))
                {
                    var fileBytes = await File.ReadAllBytesAsync(reportPath);
                    var fileName = Path.GetFileName(reportPath);
                    string contentType = GetContentType(message.OutputFormat);

                    emailMessage.Attachments.Add(new EmailAttachmentInfo
                    {
                        FileName = fileName,
                        Content = fileBytes,
                        ContentType = contentType
                    });
                }

                await _emailQueueService.QueueEmailAsync(emailMessage);
                _logger.LogInformation("Notificación de reporte completado enviada a {Email}", message.NotificationEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de reporte completado: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Envía un correo electrónico de notificación cuando falla la generación del reporte
        /// </summary>
        private async Task SendFailureNotificationEmail(ReportMessage message, string errorMessage)
        {
            try
            {
                var emailMessage = new EmailMessage
                {
                    To = message.NotificationEmail,
                    Subject = $"Error en la generación del reporte {message.ReportType}",
                    Body = $@"<html>
<body>
    <h2>Error en la Generación del Reporte</h2>
    <p>Ha ocurrido un error al generar el reporte solicitado.</p>
    <p><strong>Tipo de Reporte:</strong> {message.ReportType}</p>
    <p><strong>Formato:</strong> {message.OutputFormat}</p>
    <p><strong>Fecha del Error:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    <p><strong>Error:</strong> {errorMessage}</p>
    <p>Por favor, contacte al administrador del sistema si el problema persiste.</p>
</body>
</html>",
                    IsHtml = true,
                    SendImmediately = true
                };

                await _emailQueueService.QueueEmailAsync(emailMessage);
                _logger.LogInformation("Notificación de error en reporte enviada a {Email}", message.NotificationEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de fallo en reporte: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el tipo de contenido MIME basado en el formato del reporte
        /// </summary>
        private string GetContentType(string outputFormat)
        {
            return outputFormat.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" or "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                "html" => "text/html",
                "xml" => "application/xml",
                "json" => "application/json",
                _ => "application/octet-stream"
            };
        }
    }
}
