using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de invalidación de caché
    /// </summary>
    public interface ICacheInvalidationService
    {
        /// <summary>
        /// Invalida la caché para un usuario específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateUserCacheAsync(string userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida la caché para una organización específica
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateOrganizationCacheAsync(string organizationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida la caché para un rol específico
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateRoleCacheAsync(string roleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida la caché para un permiso específico
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidatePermissionCacheAsync(string permissionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida la caché para una configuración LDAP específica
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateLdapConfigCacheAsync(string organizationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida la caché para información de usuario LDAP
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateLdapUserInfoCacheAsync(string username, string organizationId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida toda la caché de LDAP
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateAllLdapCacheAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida toda la caché de configuración
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        Task<int> InvalidateAllConfigurationCacheAsync(CancellationToken cancellationToken = default);
    }
}
