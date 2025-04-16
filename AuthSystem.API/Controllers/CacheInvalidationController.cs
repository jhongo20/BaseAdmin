using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/cache/invalidate")]
    [Authorize(Roles = "Administrator")]
    public class CacheInvalidationController : ControllerBase
    {
        private readonly ICacheInvalidationService _cacheInvalidationService;
        private readonly ILogger<CacheInvalidationController> _logger;

        public CacheInvalidationController(
            ICacheInvalidationService cacheInvalidationService,
            ILogger<CacheInvalidationController> logger)
        {
            _cacheInvalidationService = cacheInvalidationService;
            _logger = logger;
        }

        /// <summary>
        /// Invalida la caché para un usuario específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("user/{userId}")]
        public async Task<IActionResult> InvalidateUserCache(string userId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateUserCacheAsync(userId, cancellationToken);
                return Ok(new { count, message = $"Se han invalidado {count} claves de caché para el usuario {userId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de usuario: {UserId}", userId);
                return StatusCode(500, "Error al invalidar caché de usuario");
            }
        }

        /// <summary>
        /// Invalida la caché para una organización específica
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("organization/{organizationId}")]
        public async Task<IActionResult> InvalidateOrganizationCache(string organizationId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateOrganizationCacheAsync(organizationId, cancellationToken);
                return Ok(new { count, message = $"Se han invalidado {count} claves de caché para la organización {organizationId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de organización: {OrganizationId}", organizationId);
                return StatusCode(500, "Error al invalidar caché de organización");
            }
        }

        /// <summary>
        /// Invalida la caché para un rol específico
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("role/{roleId}")]
        public async Task<IActionResult> InvalidateRoleCache(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateRoleCacheAsync(roleId, cancellationToken);
                return Ok(new { count, message = $"Se han invalidado {count} claves de caché para el rol {roleId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de rol: {RoleId}", roleId);
                return StatusCode(500, "Error al invalidar caché de rol");
            }
        }

        /// <summary>
        /// Invalida la caché para un permiso específico
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("permission/{permissionId}")]
        public async Task<IActionResult> InvalidatePermissionCache(string permissionId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidatePermissionCacheAsync(permissionId, cancellationToken);
                return Ok(new { count, message = $"Se han invalidado {count} claves de caché para el permiso {permissionId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de permiso: {PermissionId}", permissionId);
                return StatusCode(500, "Error al invalidar caché de permiso");
            }
        }

        /// <summary>
        /// Invalida la caché para una configuración LDAP específica
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("ldap/config/{organizationId}")]
        public async Task<IActionResult> InvalidateLdapConfigCache(string organizationId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateLdapConfigCacheAsync(organizationId, cancellationToken);
                return Ok(new { count, message = $"Se ha invalidado la caché de configuración LDAP para la organización {organizationId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de configuración LDAP: {OrganizationId}", organizationId);
                return StatusCode(500, "Error al invalidar caché de configuración LDAP");
            }
        }

        /// <summary>
        /// Invalida la caché para información de usuario LDAP
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("ldap/user/{username}/{organizationId}")]
        public async Task<IActionResult> InvalidateLdapUserInfoCache(string username, string organizationId, CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateLdapUserInfoCacheAsync(username, organizationId, cancellationToken);
                return Ok(new { count, message = $"Se ha invalidado la caché de información de usuario LDAP para {username} en la organización {organizationId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de información de usuario LDAP: {Username}, {OrganizationId}", username, organizationId);
                return StatusCode(500, "Error al invalidar caché de información de usuario LDAP");
            }
        }

        /// <summary>
        /// Invalida toda la caché de LDAP
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("ldap/all")]
        public async Task<IActionResult> InvalidateAllLdapCache(CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateAllLdapCacheAsync(cancellationToken);
                return Ok(new { count, message = $"Se han invalidado {count} claves de caché de LDAP" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar toda la caché de LDAP");
                return StatusCode(500, "Error al invalidar toda la caché de LDAP");
            }
        }

        /// <summary>
        /// Invalida toda la caché de configuración
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves invalidadas</returns>
        [HttpPost("configuration/all")]
        public async Task<IActionResult> InvalidateAllConfigurationCache(CancellationToken cancellationToken)
        {
            try
            {
                int count = await _cacheInvalidationService.InvalidateAllConfigurationCacheAsync(cancellationToken);
                return Ok(new { count, message = $"Se han invalidado {count} claves de caché de configuración" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar toda la caché de configuración");
                return StatusCode(500, "Error al invalidar toda la caché de configuración");
            }
        }
    }
}
