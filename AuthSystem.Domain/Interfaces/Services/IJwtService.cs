using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de tokens JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Genera un token JWT
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="email">Correo electrónico</param>
        /// <param name="roles">Roles del usuario</param>
        /// <param name="permissions">Permisos del usuario</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="branchIds">IDs de las sucursales (opcional)</param>
        /// <param name="additionalClaims">Reclamaciones adicionales (opcional)</param>
        /// <param name="expirationMinutes">Minutos de expiración (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token JWT generado</returns>
        Task<string> GenerateTokenAsync(
            Guid userId,
            string username,
            string email,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            Guid? organizationId = null,
            IEnumerable<Guid> branchIds = null,
            IDictionary<string, string> additionalClaims = null,
            int expirationMinutes = 60,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un token de actualización
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token de actualización generado</returns>
        Task<string> GenerateRefreshTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="validateLifetime">Indica si se debe validar la vida útil</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidateTokenAsync(
            string token,
            bool validateLifetime = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las reclamaciones de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Reclamaciones del token</returns>
        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el ID de usuario de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID del usuario</returns>
        Task<Guid> GetUserIdFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el nombre de usuario de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Nombre de usuario</returns>
        Task<string> GetUsernameFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los roles de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Roles del usuario</returns>
        Task<IEnumerable<string>> GetRolesFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los permisos de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permisos del usuario</returns>
        Task<IEnumerable<string>> GetPermissionsFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el ID de la organización de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID de la organización o null</returns>
        Task<Guid?> GetOrganizationIdFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los IDs de las sucursales de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>IDs de las sucursales</returns>
        Task<IEnumerable<Guid>> GetBranchIdsFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un token está en la lista de revocados
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token está revocado</returns>
        Task<bool> IsTokenRevokedAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoca un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="reason">Razón de la revocación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token fue revocado correctamente</returns>
        Task<bool> RevokeTokenAsync(
            string token,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un token para un propósito específico (activación, restablecimiento, etc.)
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="purpose">Propósito del token</param>
        /// <param name="expirationMinutes">Minutos de expiración</param>
        /// <param name="additionalData">Datos adicionales (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token generado</returns>
        Task<string> GeneratePurposeTokenAsync(
            Guid userId,
            string purpose,
            int expirationMinutes,
            IDictionary<string, string> additionalData = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida un token de propósito específico
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="purpose">Propósito esperado</param>
        /// <param name="userId">ID del usuario esperado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidatePurposeTokenAsync(
            string token,
            string purpose,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
