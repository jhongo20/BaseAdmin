using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo para la solicitud de verificación de autenticación de dos factores
    /// </summary>
    public class TwoFactorVerifyRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Código de verificación proporcionado por la aplicación de autenticación
        /// </summary>
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        public string Code { get; set; }

        /// <summary>
        /// Token de sesión temporal generado después del inicio de sesión inicial
        /// </summary>
        [Required]
        public string SessionToken { get; set; }
    }
}
