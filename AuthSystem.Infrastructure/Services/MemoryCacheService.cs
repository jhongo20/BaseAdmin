using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de caché en memoria
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ICacheMetricsService _metricsService;
        private readonly CacheSettings _cacheSettings;
        private readonly ConcurrentDictionary<string, bool> _keys = new ConcurrentDictionary<string, bool>();

        public MemoryCacheService(
            IMemoryCache memoryCache, 
            ILogger<MemoryCacheService> logger,
            ICacheMetricsService metricsService = null,
            IOptions<CacheSettings> cacheSettings = null)
        {
            _memoryCache = memoryCache;
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

                if (_memoryCache.TryGetValue(key, out object value))
                {
                    stopwatch.Stop();
                    _logger.LogTrace("Valor recuperado de la caché: {Key}", key);
                    _metricsService?.RecordHit(key, stopwatch.ElapsedMilliseconds);
                    
                    // Descomprimir si es necesario
                    if (_cacheSettings.EnableCompression && value is string stringValue)
                    {
                        return await CacheCompression.DecompressIfNeededAsync<T>(stringValue);
                    }
                    
                    return value != null ? (T)value : default;
                }

                stopwatch.Stop();
                _logger.LogTrace("Valor no encontrado en la caché: {Key}", key);
                _metricsService?.RecordMiss(key, stopwatch.ElapsedMilliseconds);
                return default;
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

                var options = new MemoryCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SlidingExpiration = slidingExpiration.Value;
                }

                // Registrar expiración
                options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    if (reason == EvictionReason.Expired)
                    {
                        _metricsService?.RecordExpiration(evictedKey.ToString());
                    }
                    else if (reason == EvictionReason.Removed)
                    {
                        _metricsService?.RecordEviction(evictedKey.ToString());
                    }
                    
                    _keys.TryRemove(evictedKey.ToString(), out _);
                    _metricsService?.UpdateItemCount(_keys.Count);
                });

                // Comprimir si es necesario
                object valueToStore = value;
                if (_cacheSettings.EnableCompression)
                {
                    string compressedValue = await CacheCompression.CompressIfNeededAsync(value);
                    valueToStore = compressedValue;
                }

                _memoryCache.Set(key, valueToStore, options);
                _keys.TryAdd(key, true);
                _metricsService?.UpdateItemCount(_keys.Count);
                
                _logger.LogTrace("Valor almacenado en la caché: {Key}", key);
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

                _memoryCache.Remove(key);
                bool removed = _keys.TryRemove(key, out _);
                
                if (removed)
                {
                    _metricsService?.RecordEviction(key);
                    _metricsService?.UpdateItemCount(_keys.Count);
                }
                
                _logger.LogTrace("Valor eliminado de la caché: {Key}", key);
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

                return _keys.ContainsKey(key);
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

                // Intentar obtener de la caché
                var cachedValue = await GetAsync<T>(key, cancellationToken);
                if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default))
                {
                    stopwatch.Stop();
                    _metricsService?.RecordHit(key, stopwatch.ElapsedMilliseconds);
                    return cachedValue;
                }

                stopwatch.Stop();
                _metricsService?.RecordMiss(key, stopwatch.ElapsedMilliseconds);

                // Obtener de la fuente
                var result = await factory();
                
                // Almacenar en caché
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

                long currentValue = 0;
                if (_memoryCache.TryGetValue(key, out object existingValue))
                {
                    if (existingValue is long longValue)
                    {
                        currentValue = longValue;
                    }
                    else if (existingValue is string stringValue && long.TryParse(stringValue, out long parsedValue))
                    {
                        currentValue = parsedValue;
                    }
                }

                long newValue = currentValue + value;
                _memoryCache.Set(key, newValue);
                _keys.TryAdd(key, true);
                _metricsService?.UpdateItemCount(_keys.Count);
                
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

                long currentValue = 0;
                if (_memoryCache.TryGetValue(key, out object existingValue))
                {
                    if (existingValue is long longValue)
                    {
                        currentValue = longValue;
                    }
                    else if (existingValue is string stringValue && long.TryParse(stringValue, out long parsedValue))
                    {
                        currentValue = parsedValue;
                    }
                }

                long newValue = currentValue - value;
                if (newValue < 0)
                {
                    newValue = 0;
                }
                
                _memoryCache.Set(key, newValue);
                _keys.TryAdd(key, true);
                _metricsService?.UpdateItemCount(_keys.Count);
                
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
        public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    throw new ArgumentException("El patrón no puede ser nulo o vacío", nameof(pattern));
                }

                // Convertir el patrón de estilo glob a regex
                string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                var regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                // Encontrar todas las claves que coinciden con el patrón
                var keysToRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();
                
                // Eliminar cada clave
                int count = 0;
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    if (_keys.TryRemove(key, out _))
                    {
                        count++;
                        _metricsService?.RecordEviction(key);
                    }
                }
                
                _metricsService?.UpdateItemCount(_keys.Count);
                _logger.LogDebug("Eliminadas {Count} claves que coinciden con el patrón: {Pattern}", count, pattern);
                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar claves por patrón: {Pattern}", pattern);
                return Task.FromResult(0);
            }
        }

        /// <inheritdoc />
        public Task<bool> ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Obtener todas las claves
                var allKeys = _keys.Keys.ToList();
                
                // Eliminar cada clave
                foreach (var key in allKeys)
                {
                    _memoryCache.Remove(key);
                    _keys.TryRemove(key, out _);
                    _metricsService?.RecordEviction(key);
                }
                
                _metricsService?.UpdateItemCount(0);
                _logger.LogDebug("Caché limpiada, eliminadas {Count} claves", allKeys.Count);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar la caché");
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                // IMemoryCache no proporciona una forma directa de obtener el tiempo de expiración
                // Solo podemos verificar si la clave existe
                if (!_keys.ContainsKey(key))
                {
                    return Task.FromResult(-2L); // -2 indica que la clave no existe (convención de Redis)
                }
                
                // No podemos determinar el tiempo de expiración exacto
                _logger.LogDebug("No se puede determinar el tiempo de expiración exacto para la clave: {Key}", key);
                return Task.FromResult(-1L); // -1 indica que la clave existe pero no tiene tiempo de expiración (convención de Redis)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tiempo de expiración: {Key}", key);
                return Task.FromResult(-2L);
            }
        }
    }
}
