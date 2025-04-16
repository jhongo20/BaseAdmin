using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de bloqueo de cuentas
    /// </summary>
    public class AccountLockoutService : IAccountLockoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountLockoutService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISecurityMonitoringService _securityMonitoringService;
        private readonly int _maxFailedAttempts;
        private readonly int _lockoutDurationMinutes;
        private readonly bool _enableAccountLockout;
        private readonly bool _enableEmailNotifications;

        public AccountLockoutService(
            IUnitOfWork unitOfWork,
            ILogger<AccountLockoutService> logger,
            IConfiguration configuration,
            IEmailService emailService,
            ISecurityMonitoringService securityMonitoringService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
            _securityMonitoringService = securityMonitoringService;
            
            // Cargar configuración
            _maxFailedAttempts = _configuration.GetValue<int>("AccountLockout:MaxFailedAttempts", 5);
            _lockoutDurationMinutes = _configuration.GetValue<int>("AccountLockout:LockoutDurationMinutes", 15);
            _enableAccountLockout = _configuration.GetValue<bool>("AccountLockout:EnableAccountLockout", true);
            _enableEmailNotifications = _configuration.GetValue<bool>("AccountLockout:EnableEmailNotifications", true);
        }

        /// <inheritdoc />
        public bool IsAccountLocked(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!_enableAccountLockout || !user.LockoutEnabled)
                return false;

            return user.IsLockedOut();
        }

        /// <inheritdoc />
        public async Task<bool> RecordFailedLoginAttemptAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!_enableAccountLockout || !user.LockoutEnabled)
                return false;

            bool isLocked = user.IncrementAccessFailedCount(_maxFailedAttempts, _lockoutDurationMinutes);
            
            if (isLocked)
            {
                _logger.LogWarning(
                    "La cuenta del usuario {Username} ha sido bloqueada por {Duration} minutos después de {Attempts} intentos fallidos",
                    user.Username, _lockoutDurationMinutes, _maxFailedAttempts);
                
                // Registrar el bloqueo en el servicio de monitoreo de seguridad
                await _securityMonitoringService.RecordAccountLockoutAsync(
                    user.Username,
                    "Múltiples intentos fallidos de inicio de sesión",
                    _lockoutDurationMinutes,
                    DateTime.UtcNow);
                
                // Enviar notificación por email si está habilitado
                if (_enableEmailNotifications && !string.IsNullOrEmpty(user.Email))
                {
                    await SendAccountLockedEmailAsync(user);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Intento de inicio de sesión fallido para el usuario {Username}. Intentos fallidos: {FailedCount}/{MaxAttempts}",
                    user.Username, user.AccessFailedCount, _maxFailedAttempts);
                
                // Registrar el intento fallido en el servicio de monitoreo de seguridad
                await _securityMonitoringService.RecordFailedLoginAttemptAsync(
                    user.Username,
                    GetClientIpAddress(),
                    GetUserAgent(),
                    DateTime.UtcNow);
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return isLocked;
        }

        /// <inheritdoc />
        public async Task RecordSuccessfulLoginAttemptAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.UpdateOnSuccessfulLogin();
            
            // Si el usuario estaba bloqueado, lo desbloqueamos
            if (user.Status == UserStatus.Locked)
            {
                user.Status = UserStatus.Active;
                user.LockoutEnd = null;
                _logger.LogInformation("La cuenta del usuario {Username} ha sido desbloqueada después de un inicio de sesión exitoso", user.Username);
            }

            // Registrar el inicio de sesión exitoso en el servicio de monitoreo de seguridad
            await _securityMonitoringService.RecordSuccessfulLoginAsync(
                user.Username,
                GetClientIpAddress(),
                GetUserAgent(),
                DateTime.UtcNow);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UnlockAccountAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            
            if (user.Status == UserStatus.Locked)
            {
                user.Status = UserStatus.Active;
            }

            _logger.LogInformation("La cuenta del usuario {Username} ha sido desbloqueada manualmente", user.Username);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Envía un correo electrónico al usuario notificando que su cuenta ha sido bloqueada
        /// </summary>
        /// <param name="user">Usuario cuya cuenta ha sido bloqueada</param>
        private async Task SendAccountLockedEmailAsync(User user)
        {
            try
            {
                string subject = "Alerta de Seguridad: Su cuenta ha sido bloqueada";
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                        h1 {{ color: #dc3545; }}
                        .info {{ background-color: #f8f9fa; padding: 15px; margin: 20px 0; border-left: 4px solid #dc3545; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Alerta de Seguridad</h1>
                        </div>
                        <div class='content'>
                            <p>Estimado/a <strong>{user.FullName}</strong>,</p>
                            
                            <p>Le informamos que su cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos de inicio de sesión.</p>
                            
                            <div class='info'>
                                <p><strong>Detalles:</strong></p>
                                <ul>
                                    <li>Usuario: {user.Username}</li>
                                    <li>Fecha y hora: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}</li>
                                    <li>Duración del bloqueo: {_lockoutDurationMinutes} minutos</li>
                                </ul>
                            </div>
                            
                            <p>Su cuenta será desbloqueada automáticamente después de {_lockoutDurationMinutes} minutos. Después de ese tiempo, podrá intentar iniciar sesión nuevamente.</p>
                            
                            <p>Si no ha sido usted quien ha intentado acceder a su cuenta, le recomendamos que cambie su contraseña inmediatamente después de que su cuenta sea desbloqueada.</p>
                            
                            <p>Si necesita asistencia inmediata, por favor contacte al administrador del sistema.</p>
                            
                            <p>Atentamente,<br>El Equipo de Seguridad</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un mensaje automático, por favor no responda a este correo.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    subject,
                    body,
                    true,
                    cancellationToken: CancellationToken.None);

                _logger.LogInformation("Se ha enviado un correo de notificación de bloqueo de cuenta a {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de notificación de bloqueo de cuenta a {Email}", user.Email);
            }
        }

        /// <summary>
        /// Obtiene la dirección IP del cliente actual
        /// </summary>
        /// <returns>Dirección IP del cliente</returns>
        private string GetClientIpAddress()
        {
            // En una implementación real, esto obtendría la IP del HttpContext actual
            // Como este es un servicio, no tenemos acceso directo al HttpContext
            // Se podría usar IHttpContextAccessor, pero para simplificar usamos una IP ficticia
            return "127.0.0.1";
        }

        /// <summary>
        /// Obtiene el User-Agent del cliente actual
        /// </summary>
        /// <returns>User-Agent del cliente</returns>
        private string GetUserAgent()
        {
            // En una implementación real, esto obtendría el User-Agent del HttpContext actual
            // Como este es un servicio, no tenemos acceso directo al HttpContext
            return "Unknown";
        }
    }
}
