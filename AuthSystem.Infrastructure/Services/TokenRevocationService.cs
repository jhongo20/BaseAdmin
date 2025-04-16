using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de revocación de tokens
    /// </summary>
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TokenRevocationService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo</param>
        /// <param name="logger">Logger</param>
        public TokenRevocationService(IUnitOfWork unitOfWork, ILogger<TokenRevocationService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
        public async Task<RevokedToken> RevokeTokenAsync(string token, Guid userId, string reason, Guid? revokedByUserId = null, string ipAddress = null, string userAgent = null)
        {
            try
            {
                // Verificar si el token ya está revocado
                bool isRevoked = await _unitOfWork.RevokedTokens.IsTokenRevokedAsync(token);
                if (isRevoked)
                {
                    _logger.LogWarning("Intento de revocar un token que ya está revocado. Usuario: {UserId}", userId);
                    return await _unitOfWork.RevokedTokens.GetByTokenAsync(token);
                }

                // Extraer información del token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                // Obtener el JTI (JWT ID) y la fecha de expiración
                string tokenId = jwtToken.Id;
                DateTime expiryDate = jwtToken.ValidTo;

                // Crear el registro de token revocado
                var revokedToken = new RevokedToken
                {
                    Token = token,
                    TokenId = tokenId,
                    ExpiryDate = expiryDate,
                    RevocationReason = reason,
                    RevokedByUserId = revokedByUserId,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                // Guardar en la base de datos
                await _unitOfWork.RevokedTokens.AddAsync(revokedToken);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Token revocado exitosamente. Usuario: {UserId}, Motivo: {Reason}", userId, reason);
                return revokedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar token. Usuario: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Revoca todos los tokens de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Motivo de la revocación</param>
        /// <param name="revokedByUserId">ID del usuario que revoca los tokens (null si es el sistema)</param>
        /// <param name="ipAddress">Dirección IP desde la que se revocan los tokens</param>
        /// <param name="userAgent">User-Agent desde el que se revocan los tokens</param>
        /// <returns>Número de tokens revocados</returns>
        public async Task<int> RevokeAllUserTokensAsync(Guid userId, string reason, Guid? revokedByUserId = null, string ipAddress = null, string userAgent = null)
        {
            try
            {
                // Obtener todas las sesiones activas del usuario
                var userSessions = await _unitOfWork.UserSessions.GetActiveByUserIdAsync(userId);
                int revokedCount = 0;

                // Revocar cada token de sesión
                foreach (var session in userSessions)
                {
                    if (!string.IsNullOrEmpty(session.AccessToken))
                    {
                        await RevokeTokenAsync(
                            session.AccessToken,
                            userId,
                            reason,
                            revokedByUserId,
                            ipAddress,
                            userAgent
                        );
                        revokedCount++;
                    }

                    // Marcar la sesión como inactiva
                    session.IsActive = false;
                    session.EndedAt = DateTime.UtcNow;
                    session.EndReason = reason;
                    await _unitOfWork.UserSessions.UpdateAsync(session);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Todos los tokens del usuario {UserId} han sido revocados. Motivo: {Reason}", userId, reason);
                return revokedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar todos los tokens del usuario {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un token está revocado
        /// </summary>
        /// <param name="token">Token JWT a verificar</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            try
            {
                return await _unitOfWork.RevokedTokens.IsTokenRevokedAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el token está revocado");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un token está revocado por su ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token (JTI)</param>
        /// <returns>True si el token está revocado, false en caso contrario</returns>
        public async Task<bool> IsTokenIdRevokedAsync(string tokenId)
        {
            try
            {
                return await _unitOfWork.RevokedTokens.IsTokenIdRevokedAsync(tokenId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el token con ID {TokenId} está revocado", tokenId);
                throw;
            }
        }

        /// <summary>
        /// Limpia los tokens revocados que ya han expirado
        /// </summary>
        /// <returns>Número de tokens eliminados</returns>
        public async Task<int> CleanupExpiredTokensAsync()
        {
            try
            {
                int deletedCount = await _unitOfWork.RevokedTokens.DeleteExpiredTokensAsync();
                _logger.LogInformation("Se han eliminado {DeletedCount} tokens revocados expirados", deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar tokens revocados expirados");
                throw;
            }
        }
    }
}
