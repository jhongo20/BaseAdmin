using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de tokens revocados
    /// </summary>
    public interface IRevokedTokenRepository : IRepository<RevokedToken>
    {
        /// <summary>
        /// Obtiene un token revocado por su valor
        /// </summary>
        /// <param name="token">Valor del token</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token revocado o null si no existe</returns>
        Task<RevokedToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un token revocado por su ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token (JTI)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token revocado o null si no existe</returns>
        Task<RevokedToken> GetByTokenIdAsync(string tokenId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los tokens revocados de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de tokens revocados</returns>
        Task<IReadOnlyList<RevokedToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un token está revocado
        /// </summary>
        /// <param name="token">Token a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un token está revocado por su ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token (JTI)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        Task<bool> IsTokenIdRevokedAsync(string tokenId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina los tokens revocados que han expirado
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de tokens eliminados</returns>
        Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}
