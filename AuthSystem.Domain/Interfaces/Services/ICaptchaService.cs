using System.Threading.Tasks;
using AuthSystem.Domain.Models.Auth;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de CAPTCHA
    /// </summary>
    public interface ICaptchaService
    {
        /// <summary>
        /// Genera un nuevo CAPTCHA
        /// </summary>
        /// <returns>Respuesta con la información del CAPTCHA</returns>
        Task<CaptchaResponse> GenerateCaptchaAsync();

        /// <summary>
        /// Verifica si un CAPTCHA es válido
        /// </summary>
        /// <param name="captchaId">Identificador del CAPTCHA</param>
        /// <param name="userResponse">Respuesta proporcionada por el usuario</param>
        /// <returns>True si el CAPTCHA es válido</returns>
        Task<bool> ValidateCaptchaAsync(string captchaId, string userResponse);

        /// <summary>
        /// Determina si se debe mostrar un CAPTCHA para un usuario específico
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Respuesta indicando si se requiere CAPTCHA</returns>
        Task<CaptchaResponse> CheckCaptchaRequirementAsync(string username);
    }
}
