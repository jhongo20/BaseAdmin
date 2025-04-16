using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
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
        private readonly ICacheMetricsService _metricsService;
        private readonly CacheSettings _cacheSettings;

        public RedisCacheService(
            IDistributedCache distributedCache, 
            ILogger<RedisCacheService> logger,
            ICacheMetricsService metricsService = null,
            IOptions<CacheSettings> cacheSettings = null)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _metricsService = metricsService;
            _cacheSettings = cacheSettings?.Value ?? new CacheSettings();
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                var cachedBytes = await _distributedCache.GetAsync(key, cancellationToken);
                if (cachedBytes == null)
                {
                    stopwatch.Stop();
                    _logger.LogTrace("Valor no encontrado en la caché: {Key}", key);
                    _metricsService?.RecordMiss(key, stopwatch.ElapsedMilliseconds);
                    return default;
                }

                // Convertir bytes a string
                string cachedString = System.Text.Encoding.UTF8.GetString(cachedBytes);
                
                // Descomprimir si es necesario
                T cachedValue;
                if (_cacheSettings.EnableCompression)
                {
                    cachedValue = await CacheCompression.DecompressIfNeededAsync<T>(cachedString);
                }
                else
                {
                    cachedValue = JsonSerializer.Deserialize<T>(cachedString);
                }
                
                stopwatch.Stop();
                _logger.LogTrace("Valor recuperado de la caché: {Key}", key);
                _metricsService?.RecordHit(key, stopwatch.ElapsedMilliseconds);
                return cachedValue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error al recuperar valor de la caché: {Key}", key);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                var options = new DistributedCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SlidingExpiration = slidingExpiration.Value;
                }

                // Comprimir si es necesario
                string serializedValue;
                if (_cacheSettings.EnableCompression)
                {
                    serializedValue = await CacheCompression.CompressIfNeededAsync(value);
                }
                else
                {
                    serializedValue = JsonSerializer.Serialize(value);
                }
                
                // Convertir a bytes
                byte[] serializedBytes = System.Text.Encoding.UTF8.GetBytes(serializedValue);
                
                await _distributedCache.SetAsync(key, serializedBytes, options, cancellationToken);
                
                _logger.LogTrace("Valor almacenado en la caché: {Key}", key);
                
                // Intentar actualizar el contador de elementos (esto es aproximado en Redis)
                try
                {
                    var exists = await ExistsAsync(key, cancellationToken);
                    if (!exists)
                    {
                        // Incrementar contador de elementos si es una nueva clave
                        var countKey = "cache:metrics:itemcount";
                        await IncrementAsync(countKey, 1, cancellationToken);
                        var count = await GetAsync<long>(countKey, cancellationToken);
                        _metricsService?.UpdateItemCount(count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo actualizar el contador de elementos de caché");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al almacenar valor en la caché: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                await _distributedCache.RemoveAsync(key, cancellationToken);
                
                _logger.LogTrace("Valor eliminado de la caché: {Key}", key);
                _metricsService?.RecordEviction(key);
                
                // Intentar actualizar el contador de elementos
                try
                {
                    var countKey = "cache:metrics:itemcount";
                    var count = await GetAsync<long>(countKey, cancellationToken);
                    if (count > 0)
                    {
                        await DecrementAsync(countKey, 1, cancellationToken);
                        count = await GetAsync<long>(countKey, cancellationToken);
                        _metricsService?.UpdateItemCount(count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo actualizar el contador de elementos de caché");
                }
                
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
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                var cachedBytes = await _distributedCache.GetAsync(key, cancellationToken);
                return cachedBytes != null;
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
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                var cachedValue = await GetAsync<T>(key, cancellationToken);
                if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default))
                {
                    stopwatch.Stop();
                    _metricsService?.RecordHit(key, stopwatch.ElapsedMilliseconds);
                    return cachedValue;
                }

                stopwatch.Stop();
                _metricsService?.RecordMiss(key, stopwatch.ElapsedMilliseconds);
                
                var result = await factory();
                await SetAsync(key, result, absoluteExpiration, slidingExpiration, cancellationToken);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error al recuperar o establecer valor en la caché: {Key}", key);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                // IDistributedCache no tiene un método de incremento atómico
                // Esta implementación no es atómica y podría tener problemas de concurrencia
                var currentValue = await GetAsync<long>(key, cancellationToken);
                var newValue = currentValue + value;
                
                await SetAsync(key, newValue, cancellationToken: cancellationToken);
                
                _logger.LogTrace("Valor incrementado en la caché: {Key}, Valor: {Value}", key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al incrementar valor en la caché: {Key}", key);
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("La clave no puede ser nula o vacía", nameof(key));
                }

                // IDistributedCache no tiene un método de decremento atómico
                // Esta implementación no es atómica y podría tener problemas de concurrencia
                var currentValue = await GetAsync<long>(key, cancellationToken);
                var newValue = currentValue - value;
                
                if (newValue < 0)
                {
                    newValue = 0;
                }
                
                await SetAsync(key, newValue, cancellationToken: cancellationToken);
                
                _logger.LogTrace("Valor decrementado en la caché: {Key}, Valor: {Value}", key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al decrementar valor en la caché: {Key}", key);
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
                _metricsService?.RecordEviction($"pattern:{pattern}");
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
                _metricsService?.UpdateItemCount(0);
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
