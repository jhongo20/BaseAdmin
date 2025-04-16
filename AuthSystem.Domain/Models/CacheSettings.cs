namespace AuthSystem.Domain.Models
{
    /// <summary>
    /// Configuración del sistema de caché
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Proveedor de caché (Redis o Memory)
        /// </summary>
        public string Provider { get; set; } = "Redis";

        /// <summary>
        /// Tiempo de expiración absoluto predeterminado en minutos
        /// </summary>
        public int DefaultAbsoluteExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Tiempo de expiración deslizante predeterminado en minutos
        /// </summary>
        public int DefaultSlidingExpirationMinutes { get; set; } = 20;

        /// <summary>
        /// Tiempo de expiración absoluto para la caché de LDAP en minutos
        /// </summary>
        public int LdapCacheAbsoluteExpirationMinutes { get; set; } = 120;

        /// <summary>
        /// Tiempo de expiración absoluto para la caché de configuración en minutos
        /// </summary>
        public int ConfigurationCacheAbsoluteExpirationMinutes { get; set; } = 240;

        /// <summary>
        /// Tiempo de expiración absoluto para la caché de usuarios en minutos
        /// </summary>
        public int UserCacheAbsoluteExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Tiempo de expiración absoluto para la caché de roles en minutos
        /// </summary>
        public int RoleCacheAbsoluteExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Tiempo de expiración absoluto para la caché de permisos en minutos
        /// </summary>
        public int PermissionCacheAbsoluteExpirationMinutes { get; set; } = 60;
    }
}
