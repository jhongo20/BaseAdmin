using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo para la solicitud de activación de cuenta
    /// </summary>
    public class AccountActivationRequest
    {
        /// <summary>
        /// Token de activación de cuenta
        /// </summary>
        [Required(ErrorMessage = "El token es obligatorio")]
        public string Token { get; set; }
    }
}
