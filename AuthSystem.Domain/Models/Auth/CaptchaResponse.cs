namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo de respuesta para la validación de CAPTCHA
    /// </summary>
    public class CaptchaResponse
    {
        /// <summary>
        /// Indica si el CAPTCHA es requerido
        /// </summary>
        public bool CaptchaRequired { get; set; }

        /// <summary>
        /// Identificador único del CAPTCHA
        /// </summary>
        public string CaptchaId { get; set; }

        /// <summary>
        /// URL de la imagen del CAPTCHA
        /// </summary>
        public string CaptchaImageUrl { get; set; }

        /// <summary>
        /// Mensaje para el usuario
        /// </summary>
        public string Message { get; set; }
    }
}
