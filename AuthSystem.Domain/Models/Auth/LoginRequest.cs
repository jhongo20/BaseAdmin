using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo para la solicitud de inicio de sesión
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Contraseña
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Indica si el usuario es de LDAP
        /// </summary>
        public bool IsLdapUser { get; set; } = false;

        /// <summary>
        /// Identificador del CAPTCHA (si es requerido)
        /// </summary>
        public string CaptchaId { get; set; }

        /// <summary>
        /// Respuesta del usuario al CAPTCHA
        /// </summary>
        public string CaptchaResponse { get; set; }
    }
}
