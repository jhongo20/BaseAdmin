using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Cargar configuración
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"];
            _smtpServer = _configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isBodyHtml = true,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            IEnumerable<EmailAttachment> attachments = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isBodyHtml;

                // Agregar CC
                if (cc != null)
                {
                    foreach (var ccAddress in cc)
                    {
                        message.CC.Add(ccAddress);
                    }
                }

                // Agregar BCC
                if (bcc != null)
                {
                    foreach (var bccAddress in bcc)
                    {
                        message.Bcc.Add(bccAddress);
                    }
                }

                // Agregar archivos adjuntos
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        if (attachment.Content != null)
                        {
                            var memoryStream = new MemoryStream(attachment.Content);
                            message.Attachments.Add(new Attachment(memoryStream, attachment.FileName, attachment.ContentType));
                        }
                        else if (!string.IsNullOrEmpty(attachment.FilePath) && File.Exists(attachment.FilePath))
                        {
                            message.Attachments.Add(new Attachment(attachment.FilePath));
                        }
                    }
                }

                using var client = new SmtpClient(_smtpServer, _smtpPort);
                client.EnableSsl = _enableSsl;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailToManyAsync(
            IEnumerable<string> to,
            string subject,
            string body,
            bool isBodyHtml = true,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            IEnumerable<EmailAttachment> attachments = null,
            CancellationToken cancellationToken = default)
        {
            if (to == null || !to.Any())
            {
                _logger.LogWarning("No recipients specified for email");
                return false;
            }

            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                
                foreach (var recipient in to)
                {
                    message.To.Add(recipient);
                }
                
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isBodyHtml;

                // Agregar CC
                if (cc != null)
                {
                    foreach (var ccAddress in cc)
                    {
                        message.CC.Add(ccAddress);
                    }
                }

                // Agregar BCC
                if (bcc != null)
                {
                    foreach (var bccAddress in bcc)
                    {
                        message.Bcc.Add(bccAddress);
                    }
                }

                // Agregar archivos adjuntos
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        if (attachment.Content != null)
                        {
                            var memoryStream = new MemoryStream(attachment.Content);
                            message.Attachments.Add(new Attachment(memoryStream, attachment.FileName, attachment.ContentType));
                        }
                        else if (!string.IsNullOrEmpty(attachment.FilePath) && File.Exists(attachment.FilePath))
                        {
                            message.Attachments.Add(new Attachment(attachment.FilePath));
                        }
                    }
                }

                using var client = new SmtpClient(_smtpServer, _smtpPort);
                client.EnableSsl = _enableSsl;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Email sent successfully to multiple recipients");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to multiple recipients");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(
            string email,
            string username,
            string fullName,
            string resetToken,
            string resetUrl,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Construir la URL completa con el token
                var completeResetUrl = $"{resetUrl}?token={resetToken}&email={WebUtility.UrlEncode(email)}";
                
                // Cargar la plantilla de correo
                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "PasswordReset.html");
                var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
                
                // Reemplazar variables en la plantilla
                template = template
                    .Replace("{{FullName}}", fullName)
                    .Replace("{{Username}}", username)
                    .Replace("{{ResetLink}}", completeResetUrl)
                    .Replace("{{ExpiryHours}}", "24"); // Configurar según necesidad
                
                // Enviar el correo
                return await SendEmailAsync(
                    email,
                    "Restablecimiento de contraseña",
                    template,
                    true,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendAccountActivationEmailAsync(
            string email,
            string username,
            string fullName,
            string activationToken,
            string activationUrl,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Construir la URL completa con el token
                var completeActivationUrl = $"{activationUrl}?token={activationToken}&email={WebUtility.UrlEncode(email)}";
                
                // Cargar la plantilla de correo
                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "AccountActivation.html");
                var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
                
                // Reemplazar variables en la plantilla
                template = template
                    .Replace("{{FullName}}", fullName)
                    .Replace("{{Username}}", username)
                    .Replace("{{ActivationLink}}", completeActivationUrl)
                    .Replace("{{ExpiryHours}}", "48"); // Configurar según necesidad
                
                // Enviar el correo
                return await SendEmailAsync(
                    email,
                    "Activación de cuenta",
                    template,
                    true,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending account activation email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendSecurityAlertEmailAsync(
            string email,
            string username,
            string fullName,
            string alertType,
            string details,
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Cargar la plantilla de correo
                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "SecurityAlert.html");
                var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
                
                // Reemplazar variables en la plantilla
                template = template
                    .Replace("{{FullName}}", fullName)
                    .Replace("{{Username}}", username)
                    .Replace("{{AlertType}}", alertType)
                    .Replace("{{Details}}", details)
                    .Replace("{{IpAddress}}", ipAddress)
                    .Replace("{{UserAgent}}", userAgent)
                    .Replace("{{DateTime}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                
                // Enviar el correo
                return await SendEmailAsync(
                    email,
                    $"Alerta de seguridad: {alertType}",
                    template,
                    true,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security alert email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(
            string email,
            string username,
            string fullName,
            string activationToken = null,
            string activationUrl = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string subject = "Bienvenido al sistema";
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4a6da7; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #4a6da7; color: white; text-decoration: none; border-radius: 5px; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Bienvenido</h2>
                        </div>
                        <div class='content'>
                            <p>Hola {fullName},</p>
                            <p>¡Bienvenido al sistema! Tu cuenta ha sido creada exitosamente.</p>
                            <p>Saludos,<br>El equipo de {_fromName}</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body, true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {To}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordChangedNotificationAsync(
            string email,
            string username,
            string fullName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string subject = "Cambio de contraseña";
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4a6da7; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .alert {{ background-color: #f8d7da; color: #721c24; padding: 10px; border-radius: 5px; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Cambio de Contraseña</h2>
                        </div>
                        <div class='content'>
                            <p>Hola {fullName},</p>
                            <p>Te informamos que la contraseña de tu cuenta ha sido cambiada recientemente.</p>
                            <p>Si tú realizaste este cambio, puedes ignorar este mensaje.</p>
                            <div class='alert'>
                                <p>Si no realizaste este cambio, por favor contacta inmediatamente con nuestro equipo de soporte, ya que tu cuenta podría estar comprometida.</p>
                            </div>
                            <p>Saludos,<br>El equipo de {_fromName}</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body, true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password changed notification to {To}", email);
                return false;
            }
        }

        public async Task<bool> SendAccountLockedNotificationAsync(
            string email,
            string username,
            string fullName,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string subject = "Cuenta bloqueada";
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #d9534f; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .alert {{ background-color: #f8d7da; color: #721c24; padding: 10px; border-radius: 5px; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Cuenta Bloqueada</h2>
                        </div>
                        <div class='content'>
                            <p>Hola {fullName},</p>
                            <div class='alert'>
                                <p>Te informamos que tu cuenta ha sido bloqueada por el siguiente motivo:</p>
                                <p><strong>{reason}</strong></p>
                            </div>
                            <p>Si crees que esto es un error o necesitas ayuda para desbloquear tu cuenta, por favor contacta con nuestro equipo de soporte.</p>
                            <p>Saludos,<br>El equipo de {_fromName}</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body, true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending account locked notification to {To}", email);
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeEmailAsync(
            string email,
            string username,
            string fullName,
            string verificationCode,
            string purpose,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string subject = $"Código de verificación para {purpose}";
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4a6da7; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .code {{ font-size: 24px; font-weight: bold; text-align: center; padding: 15px; background-color: #f8f9fa; border-radius: 5px; letter-spacing: 5px; margin: 20px 0; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Código de Verificación</h2>
                        </div>
                        <div class='content'>
                            <p>Hola {fullName},</p>
                            <p>Has solicitado un código de verificación para {purpose}.</p>
                            <p>Tu código de verificación es:</p>
                            <div class='code'>{verificationCode}</div>
                            <p>Este código expirará en 15 minutos.</p>
                            <p>Si no solicitaste este código, puedes ignorar este correo.</p>
                            <p>Saludos,<br>El equipo de {_fromName}</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(email, subject, body, true, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code email to {To}", email);
                return false;
            }
        }

        public async Task<bool> SendTemplatedEmailAsync(
            string to,
            string subject,
            string templateName,
            object templateData,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            IEnumerable<EmailAttachment> attachments = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Aquí se implementaría la lógica para cargar y renderizar plantillas
                // Por ahora, simplemente devolvemos un mensaje de error
                _logger.LogWarning("Template rendering not implemented yet. Template: {TemplateName}", templateName);
                
                string body = $@"
                <html>
                <body>
                    <p>La funcionalidad de plantillas no está implementada aún.</p>
                    <p>Plantilla solicitada: {templateName}</p>
                </body>
                </html>";

                return await SendEmailAsync(to, subject, body, true, cc, bcc, attachments, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {To}", to);
                return false;
            }
        }
    }
}
