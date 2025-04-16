using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de caché utilizando memoria
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, bool> _keys = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="memoryCache">Caché en memoria</param>
        /// <param name="logger">Logger</param>
        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T cachedValue))
                {
                    _logger.LogDebug("Valor obtenido de la caché: {Key}", key);
                    return Task.FromResult(cachedValue);
                }

                return Task.FromResult<T>(default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener valor de la caché: {Key}", key);
                return Task.FromResult<T>(default);
            }
        }

        /// <inheritdoc />
        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.SetAbsoluteExpiration(absoluteExpiration.Value);
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(slidingExpiration.Value);
                }

                options.RegisterPostEvictionCallback((k, v, reason, state) =>
                {
                    _keys.TryRemove(k.ToString(), out _);
                    _logger.LogDebug("Clave eliminada de la caché por expiración: {Key}, Razón: {Reason}", k, reason);
                });

                _memoryCache.Set(key, value, options);
                _keys.TryAdd(key, true);
                
                _logger.LogDebug("Valor establecido en la caché: {Key}", key);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer valor en la caché: {Key}", key);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                _memoryCache.Remove(key);
                _keys.TryRemove(key, out _);
                
                _logger.LogDebug("Valor eliminado de la caché: {Key}", key);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar valor de la caché: {Key}", key);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                return Task.FromResult(_keys.ContainsKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia en la caché: {Key}", key);
                return Task.FromResult(false);
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
                    }
                }
                
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
                }
                
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
