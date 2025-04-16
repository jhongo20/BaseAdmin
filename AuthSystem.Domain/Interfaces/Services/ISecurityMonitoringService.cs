using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Domain.Models.Security;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de monitoreo de seguridad
    /// </summary>
    public interface ISecurityMonitoringService
    {
        /// <summary>
        /// Registra un intento fallido de inicio de sesión
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="userAgent">User-Agent del navegador</param>
        /// <param name="timestamp">Marca de tiempo del intento</param>
        Task RecordFailedLoginAttemptAsync(string username, string ipAddress, string userAgent, DateTime timestamp);

        /// <summary>
        /// Registra un inicio de sesión exitoso
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="userAgent">User-Agent del navegador</param>
        /// <param name="timestamp">Marca de tiempo del inicio de sesión</param>
        Task RecordSuccessfulLoginAsync(string username, string ipAddress, string userAgent, DateTime timestamp);

        /// <summary>
        /// Registra un bloqueo de cuenta
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="reason">Razón del bloqueo</param>
        /// <param name="duration">Duración del bloqueo en minutos</param>
        /// <param name="timestamp">Marca de tiempo del bloqueo</param>
        Task RecordAccountLockoutAsync(string username, string reason, int duration, DateTime timestamp);

        /// <summary>
        /// Detecta patrones sospechosos de intentos de inicio de sesión
        /// </summary>
        /// <returns>Lista de alertas de seguridad</returns>
        Task<IEnumerable<SecurityAlert>> DetectSuspiciousPatterns();

        /// <summary>
        /// Obtiene las alertas de seguridad recientes
        /// </summary>
        /// <param name="count">Número máximo de alertas a obtener</param>
        /// <returns>Lista de alertas de seguridad</returns>
        Task<IEnumerable<SecurityAlert>> GetRecentAlertsAsync(int count = 10);

        /// <summary>
        /// Obtiene las estadísticas de seguridad
        /// </summary>
        /// <returns>Estadísticas de seguridad</returns>
        Task<SecurityStats> GetSecurityStatsAsync();
    }
}
