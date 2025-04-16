using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using AuthSystem.Domain.Models.MessageQueue.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.MessageQueue.Consumers
{
    /// <summary>
    /// Consumidor de mensajes de correo electrónico
    /// </summary>
    public class EmailMessageConsumer : IConsumer<EmailMessage>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailMessageConsumer> _logger;

        public EmailMessageConsumer(IEmailService emailService, ILogger<EmailMessageConsumer> logger)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Procesa un mensaje de correo electrónico
        /// </summary>
        public async Task Consume(ConsumeContext<EmailMessage> context)
        {
            var message = context.Message;
            
            try
            {
                _logger.LogInformation("Procesando mensaje de correo electrónico {MessageId} para {Recipient}", 
                    message.Id, message.To);

                // Convertir los adjuntos del mensaje a EmailAttachment
                var attachments = new List<EmailAttachment>();
                if (message.Attachments != null && message.Attachments.Any())
                {
                    foreach (var attachment in message.Attachments)
                    {
                        attachments.Add(new EmailAttachment
                        {
                            FileName = attachment.FileName,
                            Content = attachment.Content,
                            ContentType = attachment.ContentType
                        });
                    }
                }

                // Convertir destinatarios CC y BCC a listas de strings
                var ccList = !string.IsNullOrEmpty(message.Cc) 
                    ? message.Cc.Split(',').Select(e => e.Trim()).ToList() 
                    : new List<string>();
                    
                var bccList = !string.IsNullOrEmpty(message.Bcc) 
                    ? message.Bcc.Split(',').Select(e => e.Trim()).ToList() 
                    : new List<string>();

                // Enviar el correo electrónico
                await _emailService.SendEmailAsync(
                    message.To,
                    message.Subject,
                    message.Body,
                    message.IsHtml,
                    ccList,
                    bccList,
                    attachments);

                _logger.LogInformation("Correo electrónico {MessageId} enviado exitosamente a {Recipient}", 
                    message.Id, message.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar mensaje de correo electrónico {MessageId}: {ErrorMessage}", 
                    message.Id, ex.Message);
                
                // Re-encolar el mensaje para reintento si es necesario
                if (context.GetRetryCount() < 3)
                {
                    _logger.LogWarning("Reintentando envío de correo electrónico {MessageId}, intento {RetryCount}", 
                        message.Id, context.GetRetryCount() + 1);
                    throw; // MassTransit reintentará automáticamente
                }
                else
                {
                    _logger.LogError("Se alcanzó el número máximo de reintentos para el correo electrónico {MessageId}", 
                        message.Id);
                    // Aquí se podría implementar lógica para mover a una cola de mensajes fallidos
                }
            }
        }
    }
}
