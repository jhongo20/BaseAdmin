namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo de respuesta para la configuración de autenticación de dos factores
    /// </summary>
    public class TwoFactorSetupResponse
    {
        /// <summary>
        /// Clave secreta para la autenticación de dos factores
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// URL para generar el código QR
        /// </summary>
        public string QrCodeUrl { get; set; }

        /// <summary>
        /// Nombre de la aplicación para mostrar en la aplicación de autenticación
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Nombre de usuario para mostrar en la aplicación de autenticación
        /// </summary>
        public string Username { get; set; }
    }
}
