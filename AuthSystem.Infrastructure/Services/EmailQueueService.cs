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
    /// Implementación del servicio de cola de correos electrónicos
    /// </summary>
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IMessageBusService _messageBusService;
        private readonly MessageQueueSettings _settings;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(
            IMessageBusService messageBusService,
            IOptions<MessageQueueSettings> settings,
            ILogger<EmailQueueService> logger)
        {
            _messageBusService = messageBusService ?? throw new ArgumentNullException(nameof(messageBusService));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task QueueEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
        {
            var emailMessage = new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = isHtml,
                CreatedAt = DateTime.UtcNow
            };

            await QueueEmailAsync(emailMessage, cancellationToken);
        }

        /// <inheritdoc />
        public async Task QueueEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Encolando correo electrónico para {Recipient} con asunto: {Subject}", 
                    emailMessage.To, emailMessage.Subject);

                // Si el correo está programado para una fecha futura
                if (emailMessage.ScheduledFor.HasValue && emailMessage.ScheduledFor > DateTime.UtcNow)
                {
                    await _messageBusService.PublishScheduledAsync(emailMessage, emailMessage.ScheduledFor.Value, cancellationToken);
                    _logger.LogInformation("Correo electrónico programado para {ScheduledTime}", emailMessage.ScheduledFor.Value);
                }
                // Si el correo debe enviarse inmediatamente
                else if (emailMessage.SendImmediately)
                {
                    await _messageBusService.PublishAsync(emailMessage, cancellationToken);
                    _logger.LogInformation("Correo electrónico enviado para procesamiento inmediato");
                }
                // Caso normal: encolar en la cola específica de correos
                else
                {
                    await _messageBusService.PublishToQueueAsync(emailMessage, _settings.QueueNames.EmailQueue, cancellationToken);
                    _logger.LogInformation("Correo electrónico encolado exitosamente en {QueueName}", _settings.QueueNames.EmailQueue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar correo electrónico para {Recipient}: {ErrorMessage}", 
                    emailMessage.To, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task QueueBulkEmailAsync(IEnumerable<EmailMessage> emailMessages, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Encolando múltiples correos electrónicos");
                
                foreach (var emailMessage in emailMessages)
                {
                    await QueueEmailAsync(emailMessage, cancellationToken);
                }
                
                _logger.LogInformation("Múltiples correos electrónicos encolados exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar múltiples correos electrónicos: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
