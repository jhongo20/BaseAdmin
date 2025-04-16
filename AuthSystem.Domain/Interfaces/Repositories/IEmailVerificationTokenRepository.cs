using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de tokens de verificación de correo electrónico
    /// </summary>
    public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>
    {
        /// <summary>
        /// Obtiene un token por su valor
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token de verificación o null si no existe</returns>
        Task<EmailVerificationToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los tokens activos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tokens activos</returns>
        Task<IReadOnlyList<EmailVerificationToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los tokens activos para un correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tokens activos</returns>
        Task<IReadOnlyList<EmailVerificationToken>> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalida todos los tokens activos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens invalidados</returns>
        Task<int> InvalidateAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalida todos los tokens activos para un correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens invalidados</returns>
        Task<int> InvalidateAllByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un token es válido
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token es válido, false en caso contrario</returns>
        Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marca un token como utilizado
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="ipAddress">Dirección IP desde la que se utilizó</param>
        /// <param name="userAgent">User-Agent desde el que se utilizó</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token actualizado o null si no existe</returns>
        Task<EmailVerificationToken> MarkAsUsedAsync(string token, string ipAddress, string userAgent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina los tokens expirados
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens eliminados</returns>
        Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}
