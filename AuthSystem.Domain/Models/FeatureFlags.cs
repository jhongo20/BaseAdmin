namespace AuthSystem.Domain.Models
{
    /// <summary>
    /// Modelo que representa las banderas de características (feature flags) de la aplicación
    /// </summary>
    public class FeatureFlags
    {
        /// <summary>
        /// Habilita o deshabilita la API versionada
        /// </summary>
        public bool EnableVersionedApi { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita el sistema de revocación de tokens
        /// </summary>
        public bool EnableTokenRevocation { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita el sistema de sesiones distribuidas
        /// </summary>
        public bool EnableDistributedSessions { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita la autenticación de múltiples factores
        /// </summary>
        public bool EnableMultiFactorAuth { get; set; } = false;

        /// <summary>
        /// Habilita o deshabilita la integración con LDAP
        /// </summary>
        public bool EnableLdapIntegration { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita el inicio de sesión sin contraseña
        /// </summary>
        public bool EnablePasswordlessLogin { get; set; } = false;

        /// <summary>
        /// Habilita o deshabilita el inicio de sesión con redes sociales
        /// </summary>
        public bool EnableSocialLogin { get; set; } = false;

        /// <summary>
        /// Habilita o deshabilita el auto-registro de usuarios
        /// </summary>
        public bool EnableUserSelfRegistration { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita el sistema avanzado de permisos
        /// </summary>
        public bool EnableAdvancedPermissions { get; set; } = false;

        /// <summary>
        /// Habilita o deshabilita el registro de auditoría
        /// </summary>
        public bool EnableAuditLogging { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita la limitación de solicitudes
        /// </summary>
        public bool EnableRateLimiting { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita los encabezados de seguridad
        /// </summary>
        public bool EnableSecurityHeaders { get; set; } = true;

        /// <summary>
        /// Habilita o deshabilita Swagger
        /// </summary>
        public bool EnableSwagger { get; set; } = true;
    }
}
