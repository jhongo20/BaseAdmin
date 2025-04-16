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
    public class EmailQueueController : ControllerBase
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<EmailQueueController> _logger;

        public EmailQueueController(IEmailQueueService emailQueueService, ILogger<EmailQueueController> logger)
        {
            _emailQueueService = emailQueueService ?? throw new ArgumentNullException(nameof(emailQueueService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Encola un correo electrónico para su envío asíncrono
        /// </summary>
        [HttpPost("queue")]
        [Authorize(Roles = "Admin,EmailManager")]
        public async Task<IActionResult> QueueEmail([FromBody] EmailQueueRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Encolando correo electrónico para {Recipient}", request.To);

                var emailMessage = new EmailMessage
                {
                    To = request.To,
                    Cc = request.Cc,
                    Bcc = request.Bcc,
                    Subject = request.Subject,
                    Body = request.Body,
                    IsHtml = request.IsHtml,
                    Priority = request.Priority,
                    CreatedBy = User.Identity.Name,
                    SendImmediately = request.SendImmediately,
                    ScheduledFor = request.ScheduledFor
                };

                await _emailQueueService.QueueEmailAsync(emailMessage, cancellationToken);

                return Ok(new { Message = "Correo electrónico encolado exitosamente", MessageId = emailMessage.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar correo electrónico: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Error = "Error al encolar correo electrónico", Message = ex.Message });
            }
        }

        /// <summary>
        /// Encola múltiples correos electrónicos para su envío asíncrono
        /// </summary>
        [HttpPost("queue-bulk")]
        [Authorize(Roles = "Admin,EmailManager")]
        public async Task<IActionResult> QueueBulkEmail([FromBody] List<EmailQueueRequest> requests, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Encolando {Count} correos electrónicos", requests.Count);

                var emailMessages = new List<EmailMessage>();
                foreach (var request in requests)
                {
                    emailMessages.Add(new EmailMessage
                    {
                        To = request.To,
                        Cc = request.Cc,
                        Bcc = request.Bcc,
                        Subject = request.Subject,
                        Body = request.Body,
                        IsHtml = request.IsHtml,
                        Priority = request.Priority,
                        CreatedBy = User.Identity.Name,
                        SendImmediately = request.SendImmediately,
                        ScheduledFor = request.ScheduledFor
                    });
                }

                await _emailQueueService.QueueBulkEmailAsync(emailMessages, cancellationToken);

                return Ok(new { Message = $"{requests.Count} correos electrónicos encolados exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar múltiples correos electrónicos: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { Error = "Error al encolar múltiples correos electrónicos", Message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo para la solicitud de encolamiento de correo electrónico
    /// </summary>
    public class EmailQueueRequest
    {
        /// <summary>
        /// Destinatario del correo electrónico
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Destinatarios en copia
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        /// Destinatarios en copia oculta
        /// </summary>
        public string Bcc { get; set; }

        /// <summary>
        /// Asunto del correo electrónico
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Cuerpo del correo electrónico
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Indica si el cuerpo es HTML
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// Prioridad del correo electrónico (1-5, donde 1 es la más alta)
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Indica si el correo debe enviarse inmediatamente
        /// </summary>
        public bool SendImmediately { get; set; } = false;

        /// <summary>
        /// Fecha y hora programada para el envío del correo
        /// </summary>
        public DateTime? ScheduledFor { get; set; }
    }
}
