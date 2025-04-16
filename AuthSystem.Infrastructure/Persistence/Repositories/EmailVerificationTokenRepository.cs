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
    /// Implementación del repositorio de tokens de verificación de correo electrónico
    /// </summary>
    public class EmailVerificationTokenRepository : Repository<EmailVerificationToken>, IEmailVerificationTokenRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public EmailVerificationTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un token por su valor
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token de verificación o null si no existe</returns>
        public async Task<EmailVerificationToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene los tokens activos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tokens activos</returns>
        public async Task<IReadOnlyList<EmailVerificationToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiryDate > now && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene los tokens activos para un correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tokens activos</returns>
        public async Task<IReadOnlyList<EmailVerificationToken>> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(t => t.Email == email && !t.IsUsed && t.ExpiryDate > now && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Invalida todos los tokens activos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens invalidados</returns>
        public async Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var tokensToInvalidate = await _dbSet
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiryDate > now && t.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var token in tokensToInvalidate)
            {
                token.IsActive = false;
                token.LastModifiedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return tokensToInvalidate.Count;
        }

        /// <summary>
        /// Invalida todos los tokens activos para un correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens invalidados</returns>
        public async Task<int> InvalidateAllByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var tokensToInvalidate = await _dbSet
                .Where(t => t.Email == email && !t.IsUsed && t.ExpiryDate > now && t.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var token in tokensToInvalidate)
            {
                token.IsActive = false;
                token.LastModifiedAt = now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return tokensToInvalidate.Count;
        }

        /// <summary>
        /// Verifica si un token es válido
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token es válido, false en caso contrario</returns>
        public async Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .AnyAsync(t => t.Token == token && !t.IsUsed && t.ExpiryDate > now && t.IsActive, cancellationToken);
        }

        /// <summary>
        /// Marca un token como utilizado
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="ipAddress">Dirección IP desde la que se utilizó</param>
        /// <param name="userAgent">User-Agent desde el que se utilizó</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token actualizado o null si no existe</returns>
        public async Task<EmailVerificationToken> MarkAsUsedAsync(string token, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
        {
            var verificationToken = await GetByTokenAsync(token, cancellationToken);
            if (verificationToken == null || verificationToken.IsUsed || verificationToken.ExpiryDate <= DateTime.UtcNow)
            {
                return null;
            }

            verificationToken.IsUsed = true;
            verificationToken.UsedAt = DateTime.UtcNow;
            verificationToken.UsedIpAddress = ipAddress;
            verificationToken.UsedUserAgent = userAgent;
            verificationToken.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return verificationToken;
        }

        /// <summary>
        /// Elimina los tokens expirados
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens eliminados</returns>
        public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredTokens = await _dbSet
                .Where(t => t.ExpiryDate < now && t.IsActive)
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
