using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de sesiones de usuario
    /// </summary>
    public interface IUserSessionRepository : IRepository<UserSession>
    {
        /// <summary>
        /// Obtiene sesiones por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sesiones del usuario</returns>
        Task<IReadOnlyList<UserSession>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene sesiones activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sesiones activas del usuario</returns>
        Task<IReadOnlyList<UserSession>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una sesión por su token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        Task<UserSession> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una sesión por su token de actualización
        /// </summary>
        /// <param name="refreshToken">Token de actualización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        Task<UserSession> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una sesión por su ID de token (jti)
        /// </summary>
        /// <param name="tokenId">ID del token (jti)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        Task<UserSession> GetByTokenIdAsync(string tokenId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene sesiones activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sesiones activas del usuario</returns>
        Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoca todas las sesiones de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Razón de la revocación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de sesiones revocadas</returns>
        Task<int> RevokeAllSessionsForUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina sesiones expiradas
        /// </summary>
        /// <param name="olderThan">Fecha límite para considerar una sesión como expirada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de sesiones eliminadas</returns>
        Task<int> CleanupExpiredSessionsAsync(DateTime olderThan, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un token está en la lista de revocados
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado</returns>
        Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);
    }
}
