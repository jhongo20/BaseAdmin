using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Models.Auth;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación de dos factores
    /// </summary>
    public interface ITwoFactorAuthService
    {
        /// <summary>
        /// Genera una nueva configuración de autenticación de dos factores para un usuario
        /// </summary>
        /// <param name="user">Usuario para el que se generará la configuración</param>
        /// <returns>Información de configuración</returns>
        Task<TwoFactorSetupResponse> GenerateSetupInfoAsync(User user);

        /// <summary>
        /// Habilita la autenticación de dos factores para un usuario
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="verificationCode">Código de verificación proporcionado por el usuario</param>
        /// <returns>True si la configuración fue exitosa</returns>
        Task<bool> EnableTwoFactorAsync(User user, string verificationCode);

        /// <summary>
        /// Deshabilita la autenticación de dos factores para un usuario
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <returns>True si la deshabilitación fue exitosa</returns>
        Task<bool> DisableTwoFactorAsync(User user);

        /// <summary>
        /// Verifica un código de autenticación de dos factores
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="verificationCode">Código de verificación proporcionado por el usuario</param>
        /// <returns>True si el código es válido</returns>
        Task<bool> VerifyCodeAsync(User user, string verificationCode);

        /// <summary>
        /// Genera un token de sesión temporal después del inicio de sesión inicial
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <returns>Token de sesión temporal</returns>
        Task<string> GenerateSessionTokenAsync(User user);

        /// <summary>
        /// Valida un token de sesión temporal
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="sessionToken">Token de sesión temporal</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidateSessionTokenAsync(string username, string sessionToken);
    }
}
