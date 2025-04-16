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
    /// Implementación del repositorio de tokens revocados
    /// </summary>
    public class RevokedTokenRepository : Repository<RevokedToken>, IRevokedTokenRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public RevokedTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un token revocado por su valor
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token revocado o null si no existe</returns>
        public async Task<RevokedToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un token revocado por su ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token (JTI)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token revocado o null si no existe</returns>
        public async Task<RevokedToken> GetByTokenIdAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenId == tokenId && rt.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los tokens revocados de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tokens revocados</returns>
        public async Task<IReadOnlyList<RevokedToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Verifica si un token está revocado
        /// </summary>
        /// <param name="token">Token a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(rt => rt.Token == token && rt.IsActive, cancellationToken);
        }

        /// <summary>
        /// Verifica si un token está revocado por su ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token (JTI)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        public async Task<bool> IsTokenIdRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(rt => rt.TokenId == tokenId && rt.IsActive, cancellationToken);
        }

        /// <summary>
        /// Elimina los tokens revocados que han expirado
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens eliminados</returns>
        public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiryDate < now && rt.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var token in expiredTokens)
            {
                token.IsActive = false;
                token.LastModifiedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return expiredTokens.Count;
        }
    }
}
