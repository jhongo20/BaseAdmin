using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de invalidación de caché
    /// </summary>
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ICacheService _cacheService;
        private readonly ICacheMetricsService _metricsService;
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(
            ICacheService cacheService,
            ICacheMetricsService metricsService,
            ILogger<CacheInvalidationService> logger)
        {
            _cacheService = cacheService;
            _metricsService = metricsService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<int> InvalidateUserCacheAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("El ID de usuario no puede ser nulo o vacío", nameof(userId));
                }

                string pattern = $"user:{userId}:*";
                int count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                
                _logger.LogInformation("Invalidada caché de usuario: {UserId}, {Count} claves eliminadas", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de usuario: {UserId}", userId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidateOrganizationCacheAsync(string organizationId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationId))
                {
                    throw new ArgumentException("El ID de organización no puede ser nulo o vacío", nameof(organizationId));
                }

                string pattern = $"org:{organizationId}:*";
                int count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                
                _logger.LogInformation("Invalidada caché de organización: {OrganizationId}, {Count} claves eliminadas", organizationId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de organización: {OrganizationId}", organizationId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidateRoleCacheAsync(string roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    throw new ArgumentException("El ID de rol no puede ser nulo o vacío", nameof(roleId));
                }

                string pattern = $"role:{roleId}:*";
                int count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                
                _logger.LogInformation("Invalidada caché de rol: {RoleId}, {Count} claves eliminadas", roleId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de rol: {RoleId}", roleId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidatePermissionCacheAsync(string permissionId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(permissionId))
                {
                    throw new ArgumentException("El ID de permiso no puede ser nulo o vacío", nameof(permissionId));
                }

                string pattern = $"permission:{permissionId}:*";
                int count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                
                _logger.LogInformation("Invalidada caché de permiso: {PermissionId}, {Count} claves eliminadas", permissionId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de permiso: {PermissionId}", permissionId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidateLdapConfigCacheAsync(string organizationId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationId))
                {
                    throw new ArgumentException("El ID de organización no puede ser nulo o vacío", nameof(organizationId));
                }

                string key = $"ldap:config:{organizationId}";
                bool removed = await _cacheService.RemoveAsync(key, cancellationToken);
                
                _logger.LogInformation("Invalidada caché de configuración LDAP: {OrganizationId}, Eliminada: {Removed}", organizationId, removed);
                return removed ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de configuración LDAP: {OrganizationId}", organizationId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidateLdapUserInfoCacheAsync(string username, string organizationId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    throw new ArgumentException("El nombre de usuario no puede ser nulo o vacío", nameof(username));
                }

                if (string.IsNullOrEmpty(organizationId))
                {
                    throw new ArgumentException("El ID de organización no puede ser nulo o vacío", nameof(organizationId));
                }

                string key = $"ldap:userinfo:{organizationId}:{username}";
                bool removed = await _cacheService.RemoveAsync(key, cancellationToken);
                
                _logger.LogInformation("Invalidada caché de información de usuario LDAP: {Username}, {OrganizationId}, Eliminada: {Removed}", username, organizationId, removed);
                return removed ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de información de usuario LDAP: {Username}, {OrganizationId}", username, organizationId);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidateAllLdapCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                string pattern = "ldap:*";
                int count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                
                _logger.LogInformation("Invalidada toda la caché de LDAP, {Count} claves eliminadas", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar toda la caché de LDAP");
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> InvalidateAllConfigurationCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                string pattern = "config:*";
                int count = await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
                
                _logger.LogInformation("Invalidada toda la caché de configuración, {Count} claves eliminadas", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar toda la caché de configuración");
                return 0;
            }
        }
    }
}
