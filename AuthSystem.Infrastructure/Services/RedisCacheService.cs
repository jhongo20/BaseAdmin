using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de caché utilizando Redis
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="distributedCache">Caché distribuida</param>
        /// <param name="logger">Logger</param>
        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedBytes = await _distributedCache.GetAsync(key, cancellationToken);
                if (cachedBytes == null || cachedBytes.Length == 0)
                {
                    return default;
                }

                var cachedValue = JsonSerializer.Deserialize<T>(cachedBytes);
                _logger.LogDebug("Valor obtenido de la caché: {Key}", key);
                return cachedValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener valor de la caché: {Key}", key);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.SetAbsoluteExpiration(absoluteExpiration.Value);
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(slidingExpiration.Value);
                }

                var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
                await _distributedCache.SetAsync(key, serializedValue, options, cancellationToken);
                
                _logger.LogDebug("Valor establecido en la caché: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer valor en la caché: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Valor eliminado de la caché: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar valor de la caché: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedBytes = await _distributedCache.GetAsync(key, cancellationToken);
                return cachedBytes != null && cachedBytes.Length > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia en la caché: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            var cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default))
            {
                _logger.LogDebug("Valor obtenido de la caché: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Valor no encontrado en caché, obteniendo de la fuente: {Key}", key);
            var newValue = await factory();
            
            if (newValue != null)
            {
                await SetAsync(key, newValue, absoluteExpiration, slidingExpiration, cancellationToken);
            }
            
            return newValue;
        }

        /// <inheritdoc />
        public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentValue = await GetAsync<long>(key, cancellationToken);
                var newValue = currentValue + value;
                
                await SetAsync(key, newValue, cancellationToken: cancellationToken);
                
                _logger.LogDebug("Contador incrementado en la caché: {Key}, Valor: {Value}", key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al incrementar contador en la caché: {Key}", key);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentValue = await GetAsync<long>(key, cancellationToken);
                var newValue = currentValue - value;
                
                if (newValue < 0)
                {
                    newValue = 0;
                }
                
                await SetAsync(key, newValue, cancellationToken: cancellationToken);
                
                _logger.LogDebug("Contador decrementado en la caché: {Key}, Valor: {Value}", key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al decrementar contador en la caché: {Key}", key);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RefreshExpirationAsync(string key, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedValue = await GetAsync<object>(key, cancellationToken);
                if (cachedValue == null)
                {
                    _logger.LogWarning("No se puede actualizar la expiración, la clave no existe: {Key}", key);
                    return false;
                }

                await SetAsync(key, cachedValue, absoluteExpiration, slidingExpiration, cancellationToken);
                
                _logger.LogDebug("Expiración actualizada en la caché: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar expiración en la caché: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                // Esta implementación es limitada ya que IDistributedCache no proporciona una forma directa
                // de eliminar por patrón. En una implementación real con Redis, se utilizaría
                // el comando SCAN + DEL de Redis.
                _logger.LogWarning("RemoveByPatternAsync no está completamente implementado para IDistributedCache");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar claves por patrón: {Pattern}", pattern);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Esta implementación es limitada ya que IDistributedCache no proporciona una forma directa
                // de limpiar toda la caché. En una implementación real con Redis, se utilizaría
                // el comando FLUSHDB de Redis.
                _logger.LogWarning("ClearAsync no está completamente implementado para IDistributedCache");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar la caché");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                // Esta implementación es limitada ya que IDistributedCache no proporciona una forma directa
                // de obtener el tiempo de expiración. En una implementación real con Redis, se utilizaría
                // el comando TTL de Redis.
                var exists = await ExistsAsync(key, cancellationToken);
                if (!exists)
                {
                    return -2; // -2 indica que la clave no existe (convención de Redis)
                }
                
                _logger.LogWarning("GetTimeToLiveAsync no está completamente implementado para IDistributedCache");
                return -1; // -1 indica que la clave existe pero no tiene tiempo de expiración (convención de Redis)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tiempo de expiración: {Key}", key);
                return -2;
            }
        }
    }
}
