using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.MessageQueue.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportQueueController : ControllerBase
    {
        private readonly IReportQueueService _reportQueueService;
        private readonly ILogger<ReportQueueController> _logger;

        public ReportQueueController(IReportQueueService reportQueueService, ILogger<ReportQueueController> logger)
        {
            _reportQueueService = reportQueueService ?? throw new ArgumentNullException(nameof(reportQueueService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Encola un reporte para su generación asíncrona
        /// </summary>
        [HttpPost("queue")]
        [Authorize(Roles = "Admin,ReportManager")]
        public async Task<IActionResult> QueueReport([FromBody] ReportQueueRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Encolando reporte de tipo {ReportType}", request.ReportType);

                var reportMessage = new ReportMessage
                {
                    ReportType = request.ReportType,
                    Parameters = request.Parameters,
                    OutputFormat = request.OutputFormat,
                    RequestedBy = User.Identity.Name,
                    NotifyOnCompletion = request.NotifyOnCompletion,
                    NotificationEmail = request.NotificationEmail,
                    CallbackUrl = request.CallbackUrl,
                    Priority = request.Priority,
                    ScheduledFor = request.ScheduledFor,
                    OutputPath = request.OutputPath
                };

                await _reportQueueService.QueueReportAsync(reportMessage, cancellationToken);

                return Ok(new { Message = "Reporte encolado exitosamente", ReportId = reportMessage.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar reporte: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Error = "Error al encolar reporte", Message = ex.Message });
            }
        }

        /// <summary>
        /// Encola múltiples reportes para su generación asíncrona
        /// </summary>
        [HttpPost("queue-bulk")]
        [Authorize(Roles = "Admin,ReportManager")]
        public async Task<IActionResult> QueueBulkReport([FromBody] List<ReportQueueRequest> requests, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Encolando {Count} reportes", requests.Count);

                var reportMessages = new List<ReportMessage>();
                foreach (var request in requests)
                {
                    reportMessages.Add(new ReportMessage
                    {
                        ReportType = request.ReportType,
                        Parameters = request.Parameters,
                        OutputFormat = request.OutputFormat,
                        RequestedBy = User.Identity.Name,
                        NotifyOnCompletion = request.NotifyOnCompletion,
                        NotificationEmail = request.NotificationEmail,
                        CallbackUrl = request.CallbackUrl,
                        Priority = request.Priority,
                        ScheduledFor = request.ScheduledFor,
                        OutputPath = request.OutputPath
                    });
                }

                await _reportQueueService.QueueBulkReportAsync(reportMessages, cancellationToken);

                return Ok(new { Message = $"{requests.Count} reportes encolados exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar múltiples reportes: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Error = "Error al encolar múltiples reportes", Message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo para la solicitud de encolamiento de reporte
    /// </summary>
    public class ReportQueueRequest
    {
        /// <summary>
        /// Tipo de reporte a generar
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Parámetros para la generación del reporte
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Formato de salida del reporte (PDF, Excel, etc.)
        /// </summary>
        public string OutputFormat { get; set; } = "PDF";

        /// <summary>
        /// Indica si se debe notificar cuando el reporte esté listo
        /// </summary>
        public bool NotifyOnCompletion { get; set; } = true;

        /// <summary>
        /// Correo electrónico para notificar cuando el reporte esté listo
        /// </summary>
        public string NotificationEmail { get; set; }

        /// <summary>
        /// URL de callback para notificar cuando el reporte esté listo
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Prioridad del reporte (1-5, donde 1 es la más alta)
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Fecha y hora programada para la generación del reporte
        /// </summary>
        public DateTime? ScheduledFor { get; set; }

        /// <summary>
        /// Ruta de salida donde se guardará el reporte
        /// </summary>
        public string OutputPath { get; set; }
    }
}
