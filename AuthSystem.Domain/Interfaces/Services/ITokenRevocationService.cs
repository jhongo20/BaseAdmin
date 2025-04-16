using System;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de revocación de tokens
    /// </summary>
    public interface ITokenRevocationService
    {
        /// <summary>
        /// Revoca un token JWT
        /// </summary>
        /// <param name="token">Token JWT a revocar</param>
        /// <param name="userId">ID del usuario al que pertenece el token</param>
        /// <param name="reason">Motivo de la revocación</param>
        /// <param name="revokedByUserId">ID del usuario que revoca el token (null si es el sistema)</param>
        /// <param name="ipAddress">Dirección IP desde la que se revoca el token</param>
        /// <param name="userAgent">User-Agent desde el que se revoca el token</param>
        /// <returns>Token revocado</returns>
        Task<RevokedToken> RevokeTokenAsync(string token, Guid userId, string reason, Guid? revokedByUserId = null, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// Revoca todos los tokens de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Motivo de la revocación</param>
        /// <param name="revokedByUserId">ID del usuario que revoca los tokens (null si es el sistema)</param>
        /// <param name="ipAddress">Dirección IP desde la que se revocan los tokens</param>
        /// <param name="userAgent">User-Agent desde el que se revocan los tokens</param>
        /// <returns>Número de tokens revocados</returns>
        Task<int> RevokeAllUserTokensAsync(Guid userId, string reason, Guid? revokedByUserId = null, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// Verifica si un token está revocado
        /// </summary>
        /// <param name="token">Token JWT a verificar</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        Task<bool> IsTokenRevokedAsync(string token);

        /// <summary>
        /// Verifica si un token está revocado por su ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token (JTI)</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        Task<bool> IsTokenIdRevokedAsync(string tokenId);

        /// <summary>
        /// Limpia los tokens revocados que ya han expirado
        /// </summary>
        /// <returns>Número de tokens eliminados</returns>
        Task<int> CleanupExpiredTokensAsync();
    }
}
