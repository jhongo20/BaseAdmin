using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de sesiones de usuario
    /// </summary>
    public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UserSessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene sesiones por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sesiones del usuario</returns>
        public async Task<IReadOnlyList<UserSession>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(us => us.User)
                .Where(us => us.UserId == userId && us.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene sesiones activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sesiones activas del usuario</returns>
        public async Task<IReadOnlyList<UserSession>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(us => us.User)
                .Where(us => us.UserId == userId && us.IsActive && !us.IsRevoked && us.ExpiresAt > now)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una sesión por su token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        public async Task<UserSession> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("El token no puede ser nulo o vacío", nameof(token));
            }

            return await _dbSet
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.Token == token && us.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene una sesión por su token de actualización
        /// </summary>
        /// <param name="refreshToken">Token de actualización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        public async Task<UserSession> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("El token de actualización no puede ser nulo o vacío", nameof(refreshToken));
            }

            return await _dbSet
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.RefreshToken == refreshToken && us.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene una sesión por su ID de token (jti)
        /// </summary>
        /// <param name="tokenId">ID del token (jti)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        public async Task<UserSession> GetByTokenIdAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                throw new ArgumentException("El ID del token no puede ser nulo o vacío", nameof(tokenId));
            }

            return await _dbSet
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.TokenId == tokenId && us.IsActive, cancellationToken);
        }

        /// <summary>
        /// Revoca todas las sesiones de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Razón de la revocación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de sesiones revocadas</returns>
        public async Task<int> RevokeAllSessionsForUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var sessions = await _dbSet
                .Where(us => us.UserId == userId && us.IsActive && !us.IsRevoked && us.ExpiresAt > now)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.IsRevoked = true;
                session.LastModifiedAt = now;
                session.RevocationReason = reason;
            }

            return sessions.Count;
        }

        /// <summary>
        /// Elimina sesiones expiradas
        /// </summary>
        /// <param name="olderThan">Fecha límite para considerar una sesión como expirada</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de sesiones eliminadas</returns>
        public async Task<int> CleanupExpiredSessionsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
        {
            var expiredSessions = await _dbSet
                .Where(us => us.ExpiresAt < olderThan)
                .ToListAsync(cancellationToken);

            _dbSet.RemoveRange(expiredSessions);
            return expiredSessions.Count;
        }

        /// <summary>
        /// Verifica si un token está en la lista de revocados
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado</returns>
        public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("El token no puede ser nulo o vacío", nameof(token));
            }

            var session = await _dbSet
                .FirstOrDefaultAsync(us => us.Token == token && us.IsActive, cancellationToken);

            return session != null && session.IsRevoked;
        }

        /// <summary>
        /// Obtiene una sesión por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID de la sesión</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión encontrada o null</returns>
        public override async Task<UserSession> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(us => us.User)
                .FirstOrDefaultAsync(us => us.Id == id && us.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las sesiones activas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sesiones activas</returns>
        public override async Task<IReadOnlyList<UserSession>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(us => us.User)
                .Where(us => us.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
