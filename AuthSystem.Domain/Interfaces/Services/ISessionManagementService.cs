using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de sesiones
    /// </summary>
    public interface ISessionManagementService
    {
        /// <summary>
        /// Crea una nueva sesión para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="token">Token JWT</param>
        /// <param name="tokenId">ID del token JWT (JTI)</param>
        /// <param name="refreshToken">Token de actualización</param>
        /// <param name="expiresAt">Fecha y hora de expiración del token</param>
        /// <param name="ipAddress">Dirección IP desde la que se inicia la sesión</param>
        /// <param name="userAgent">User-Agent desde el que se inicia la sesión</param>
        /// <returns>Sesión creada</returns>
        Task<UserSession> CreateSessionAsync(Guid userId, string token, string tokenId, string refreshToken, DateTime expiresAt, string ipAddress, string userAgent);

        /// <summary>
        /// Obtiene una sesión por su ID
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <returns>Sesión o null si no existe</returns>
        Task<UserSession> GetSessionByIdAsync(Guid sessionId);

        /// <summary>
        /// Obtiene una sesión por su token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Sesión o null si no existe</returns>
        Task<UserSession> GetSessionByTokenAsync(string token);

        /// <summary>
        /// Obtiene una sesión por su token ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token JWT (JTI)</param>
        /// <returns>Sesión o null si no existe</returns>
        Task<UserSession> GetSessionByTokenIdAsync(string tokenId);

        /// <summary>
        /// Obtiene todas las sesiones activas de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de sesiones activas</returns>
        Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId);

        /// <summary>
        /// Obtiene todas las sesiones activas
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de sesiones activas</returns>
        Task<(IReadOnlyList<UserSession> Sessions, int TotalCount)> GetAllActiveSessionsAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// Obtiene el número total de sesiones activas
        /// </summary>
        /// <returns>Número total de sesiones activas</returns>
        Task<int> GetTotalActiveSessionsAsync();

        /// <summary>
        /// Obtiene el número de sesiones activas por rol
        /// </summary>
        /// <returns>Diccionario con el número de sesiones por rol</returns>
        Task<Dictionary<string, int>> GetActiveSessionsByRoleAsync();

        /// <summary>
        /// Obtiene el número de nuevas sesiones en las últimas horas
        /// </summary>
        /// <param name="hours">Número de horas hacia atrás</param>
        /// <returns>Número de nuevas sesiones</returns>
        Task<int> GetNewSessionsInLastHoursAsync(int hours);

        /// <summary>
        /// Actualiza la actividad de una sesión
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <returns>True si la sesión se actualizó correctamente, false en caso contrario</returns>
        Task<bool> UpdateSessionActivityAsync(Guid sessionId);

        /// <summary>
        /// Cierra una sesión
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <param name="reason">Motivo del cierre</param>
        /// <returns>True si la sesión se cerró correctamente, false en caso contrario</returns>
        Task<bool> CloseSessionAsync(Guid sessionId, string reason);

        /// <summary>
        /// Cierra todas las sesiones de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Motivo del cierre</param>
        /// <param name="excludeSessionId">ID de la sesión a excluir (opcional)</param>
        /// <returns>Número de sesiones cerradas</returns>
        Task<int> CloseAllUserSessionsAsync(Guid userId, string reason, Guid? excludeSessionId = null);

        /// <summary>
        /// Cierra todas las sesiones excepto la actual
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="currentSessionId">ID de la sesión actual</param>
        /// <param name="reason">Motivo del cierre</param>
        /// <returns>Número de sesiones cerradas</returns>
        Task<int> CloseAllOtherSessionsAsync(Guid userId, Guid currentSessionId, string reason);

        /// <summary>
        /// Verifica si un usuario ha excedido el número máximo de sesiones concurrentes
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>True si el usuario ha excedido el límite, false en caso contrario</returns>
        Task<bool> HasExceededMaxConcurrentSessionsAsync(Guid userId);

        /// <summary>
        /// Limpia las sesiones expiradas
        /// </summary>
        /// <returns>Número de sesiones eliminadas</returns>
        Task<int> CleanupExpiredSessionsAsync();
    }
}
