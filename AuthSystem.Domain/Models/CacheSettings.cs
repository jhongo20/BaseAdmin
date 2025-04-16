using System;

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
        public string Provider { get; set; } = "Memory";
        
        /// <summary>
        /// Tiempo de expiración absoluto predeterminado en minutos
        /// </summary>
        public int DefaultAbsoluteExpirationMinutes { get; set; } = 60;
        
        /// <summary>
        /// Tiempo de expiración deslizante predeterminado en minutos
        /// </summary>
        public int DefaultSlidingExpirationMinutes { get; set; } = 20;
        
        /// <summary>
        /// Tiempo de expiración para la caché de LDAP en minutos
        /// </summary>
        public int LdapCacheAbsoluteExpirationMinutes { get; set; } = 120;
        
        /// <summary>
        /// Tiempo de expiración para la caché de configuración en minutos
        /// </summary>
        public int ConfigurationCacheAbsoluteExpirationMinutes { get; set; } = 240;
        
        /// <summary>
        /// Tiempo de expiración para la caché de usuarios en minutos
        /// </summary>
        public int UserCacheAbsoluteExpirationMinutes { get; set; } = 30;
        
        /// <summary>
        /// Tiempo de expiración para la caché de roles en minutos
        /// </summary>
        public int RoleCacheAbsoluteExpirationMinutes { get; set; } = 60;
        
        /// <summary>
        /// Tiempo de expiración para la caché de permisos en minutos
        /// </summary>
        public int PermissionCacheAbsoluteExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Habilitar compresión de datos en caché
        /// </summary>
        public bool EnableCompression { get; set; } = true;
        
        /// <summary>
        /// Umbral de tamaño en bytes para comprimir datos (predeterminado: 1KB)
        /// </summary>
        public int CompressionThresholdBytes { get; set; } = 1024;
        
        /// <summary>
        /// Obtiene el tiempo de expiración absoluto para la caché de LDAP
        /// </summary>
        public TimeSpan LdapCacheAbsoluteExpiration => TimeSpan.FromMinutes(LdapCacheAbsoluteExpirationMinutes);
        
        /// <summary>
        /// Obtiene el tiempo de expiración absoluto para la caché de configuración
        /// </summary>
        public TimeSpan ConfigurationCacheAbsoluteExpiration => TimeSpan.FromMinutes(ConfigurationCacheAbsoluteExpirationMinutes);
        
        /// <summary>
        /// Obtiene el tiempo de expiración absoluto para la caché de usuarios
        /// </summary>
        public TimeSpan UserCacheAbsoluteExpiration => TimeSpan.FromMinutes(UserCacheAbsoluteExpirationMinutes);
        
        /// <summary>
        /// Obtiene el tiempo de expiración absoluto para la caché de roles
        /// </summary>
        public TimeSpan RoleCacheAbsoluteExpiration => TimeSpan.FromMinutes(RoleCacheAbsoluteExpirationMinutes);
        
        /// <summary>
        /// Obtiene el tiempo de expiración absoluto para la caché de permisos
        /// </summary>
        public TimeSpan PermissionCacheAbsoluteExpiration => TimeSpan.FromMinutes(PermissionCacheAbsoluteExpirationMinutes);
        
        /// <summary>
        /// Obtiene el tiempo de expiración absoluto predeterminado
        /// </summary>
        public TimeSpan DefaultAbsoluteExpiration => TimeSpan.FromMinutes(DefaultAbsoluteExpirationMinutes);
        
        /// <summary>
        /// Obtiene el tiempo de expiración deslizante predeterminado
        /// </summary>
        public TimeSpan DefaultSlidingExpiration => TimeSpan.FromMinutes(DefaultSlidingExpirationMinutes);
    }
}
