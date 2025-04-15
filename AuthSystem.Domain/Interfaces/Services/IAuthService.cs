using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica a un usuario con credenciales locales
        /// </summary>
        /// <param name="username">Nombre de usuario o correo electrónico</param>
        /// <param name="password">Contraseña</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión de usuario creada si la autenticación es exitosa</returns>
        Task<UserSession> AuthenticateLocalAsync(
            string username, 
            string password, 
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Autentica a un usuario con LDAP/Active Directory
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="password">Contraseña</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sesión de usuario creada si la autenticación es exitosa</returns>
        Task<UserSession> AuthenticateLdapAsync(
            string username, 
            string password, 
            Guid organizationId,
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un token de acceso usando un token de actualización
        /// </summary>
        /// <param name="refreshToken">Token de actualización</param>
        /// <param name="ipAddress">Dirección IP del cliente</param>
        /// <param name="userAgent">Agente de usuario del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Nueva sesión de usuario con tokens actualizados</returns>
        Task<UserSession> RefreshTokenAsync(
            string refreshToken, 
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cierra la sesión de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="token">Token JWT a revocar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la sesión se cerró correctamente</returns>
        Task<bool> LogoutAsync(
            Guid userId, 
            string token, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoca todas las sesiones activas de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Razón de la revocación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de sesiones revocadas</returns>
        Task<int> RevokeAllSessionsAsync(
            Guid userId, 
            string reason, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un token es válido
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="validateExpiration">Indica si se debe validar la expiración</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidateTokenAsync(
            string token, 
            bool validateExpiration = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el ID de usuario a partir de un token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>ID del usuario</returns>
        Task<Guid> GetUserIdFromTokenAsync(
            string token, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los permisos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del usuario</returns>
        Task<IReadOnlyList<string>> GetUserPermissionsAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="currentPassword">Contraseña actual</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el cambio fue exitoso</returns>
        Task<bool> ChangePasswordAsync(
            Guid userId, 
            string currentPassword, 
            string newPassword, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera un token para restablecer la contraseña
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Token generado</returns>
        Task<string> GeneratePasswordResetTokenAsync(
            string email, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Restablece la contraseña de un usuario usando un token
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <param name="token">Token de restablecimiento</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el restablecimiento fue exitoso</returns>
        Task<bool> ResetPasswordAsync(
            string email, 
            string token, 
            string newPassword, 
            CancellationToken cancellationToken = default);
    }
}
