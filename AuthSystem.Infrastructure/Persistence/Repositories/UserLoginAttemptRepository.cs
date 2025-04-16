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
    /// Implementación del repositorio de intentos de inicio de sesión
    /// </summary>
    public class UserLoginAttemptRepository : Repository<UserLoginAttempt>, IUserLoginAttemptRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UserLoginAttemptRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene los intentos de inicio de sesión de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de intentos de inicio de sesión</returns>
        public async Task<IReadOnlyList<UserLoginAttempt>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene los intentos de inicio de sesión fallidos recientes de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de intentos de inicio de sesión fallidos</returns>
        public async Task<IReadOnlyList<UserLoginAttempt>> GetRecentFailedByUserIdAsync(Guid userId, int minutes = 30, CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
            return await _dbSet
                .Where(a => a.UserId == userId && !a.Successful && a.CreatedAt >= cutoffTime && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene los intentos de inicio de sesión fallidos recientes desde una dirección IP
        /// </summary>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de intentos de inicio de sesión fallidos</returns>
        public async Task<IReadOnlyList<UserLoginAttempt>> GetRecentFailedByIpAddressAsync(string ipAddress, int minutes = 30, CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
            return await _dbSet
                .Where(a => a.IpAddress == ipAddress && !a.Successful && a.CreatedAt >= cutoffTime && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Cuenta los intentos de inicio de sesión fallidos recientes de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de intentos fallidos</returns>
        public async Task<int> CountRecentFailedByUserIdAsync(Guid userId, int minutes = 30, CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
            return await _dbSet
                .CountAsync(a => a.UserId == userId && !a.Successful && a.CreatedAt >= cutoffTime && a.IsActive, cancellationToken);
        }

        /// <summary>
        /// Cuenta los intentos de inicio de sesión fallidos recientes desde una dirección IP
        /// </summary>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de intentos fallidos</returns>
        public async Task<int> CountRecentFailedByIpAddressAsync(string ipAddress, int minutes = 30, CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
            return await _dbSet
                .CountAsync(a => a.IpAddress == ipAddress && !a.Successful && a.CreatedAt >= cutoffTime && a.IsActive, cancellationToken);
        }
    }
}
