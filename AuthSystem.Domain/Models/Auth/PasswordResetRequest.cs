using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo para la solicitud de restablecimiento de contraseña
    /// </summary>
    public class PasswordResetRequest
    {
        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; }
    }
}
