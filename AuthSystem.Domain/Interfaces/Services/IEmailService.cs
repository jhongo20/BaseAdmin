using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Models;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de correo electrónico
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un correo electrónico
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo (HTML)</param>
        /// <param name="isBodyHtml">Indica si el cuerpo es HTML</param>
        /// <param name="cc">Direcciones en copia (opcional)</param>
        /// <param name="bcc">Direcciones en copia oculta (opcional)</param>
        /// <param name="attachments">Archivos adjuntos (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isBodyHtml = true,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            IEnumerable<EmailAttachment> attachments = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo electrónico a múltiples destinatarios
        /// </summary>
        /// <param name="to">Direcciones de correo de los destinatarios</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="body">Cuerpo del correo (HTML)</param>
        /// <param name="isBodyHtml">Indica si el cuerpo es HTML</param>
        /// <param name="cc">Direcciones en copia (opcional)</param>
        /// <param name="bcc">Direcciones en copia oculta (opcional)</param>
        /// <param name="attachments">Archivos adjuntos (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendEmailToManyAsync(
            IEnumerable<string> to,
            string subject,
            string body,
            bool isBodyHtml = true,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            IEnumerable<EmailAttachment> attachments = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo de bienvenida a un nuevo usuario
        /// </summary>
        /// <param name="email">Dirección de correo del usuario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="activationToken">Token de activación (opcional)</param>
        /// <param name="activationUrl">URL de activación (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendWelcomeEmailAsync(
            string email,
            string username,
            string fullName,
            string activationToken = null,
            string activationUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo para restablecer la contraseña
        /// </summary>
        /// <param name="email">Dirección de correo del usuario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="resetToken">Token de restablecimiento</param>
        /// <param name="resetUrl">URL de restablecimiento</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendPasswordResetEmailAsync(
            string email,
            string username,
            string fullName,
            string resetToken,
            string resetUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo de notificación de cambio de contraseña
        /// </summary>
        /// <param name="email">Dirección de correo del usuario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendPasswordChangedNotificationAsync(
            string email,
            string username,
            string fullName,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo de notificación de bloqueo de cuenta
        /// </summary>
        /// <param name="email">Dirección de correo del usuario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="reason">Razón del bloqueo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendAccountLockedNotificationAsync(
            string email,
            string username,
            string fullName,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo con un código de verificación
        /// </summary>
        /// <param name="email">Dirección de correo del usuario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="verificationCode">Código de verificación</param>
        /// <param name="purpose">Propósito del código (activación, restablecimiento, etc.)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendVerificationCodeEmailAsync(
            string email,
            string username,
            string fullName,
            string verificationCode,
            string purpose,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo electrónico de activación de cuenta
        /// </summary>
        /// <param name="email">Dirección de correo del destinatario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="activationToken">Token de activación</param>
        /// <param name="activationUrl">URL de activación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendAccountActivationEmailAsync(
            string email,
            string username,
            string fullName,
            string activationToken,
            string activationUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo electrónico de alerta de seguridad
        /// </summary>
        /// <param name="email">Dirección de correo del destinatario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="alertType">Tipo de alerta</param>
        /// <param name="details">Detalles adicionales de la alerta</param>
        /// <param name="ipAddress">Dirección IP desde donde se realizó la acción</param>
        /// <param name="userAgent">Información del navegador/dispositivo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendSecurityAlertEmailAsync(
            string email,
            string username,
            string fullName,
            string alertType,
            string details,
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un correo usando una plantilla
        /// </summary>
        /// <param name="to">Dirección de correo del destinatario</param>
        /// <param name="subject">Asunto del correo</param>
        /// <param name="templateName">Nombre de la plantilla</param>
        /// <param name="templateData">Datos para la plantilla</param>
        /// <param name="cc">Direcciones en copia (opcional)</param>
        /// <param name="bcc">Direcciones en copia oculta (opcional)</param>
        /// <param name="attachments">Archivos adjuntos (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el correo se envió correctamente</returns>
        Task<bool> SendTemplatedEmailAsync(
            string to,
            string subject,
            string templateName,
            object templateData,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            IEnumerable<EmailAttachment> attachments = null,
            CancellationToken cancellationToken = default);
    }
}
