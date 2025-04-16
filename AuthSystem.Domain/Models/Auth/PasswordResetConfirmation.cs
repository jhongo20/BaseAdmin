using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo para confirmar el restablecimiento de contraseña
    /// </summary>
    public class PasswordResetConfirmation
    {
        /// <summary>
        /// Token de restablecimiento de contraseña
        /// </summary>
        [Required(ErrorMessage = "El token es obligatorio")]
        public string Token { get; set; }

        /// <summary>
        /// Nueva contraseña del usuario
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una letra mayúscula, un número y un carácter especial")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña
        /// </summary>
        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}
