using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de caché
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Obtiene un valor de la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Valor almacenado o default(T) si no existe</returns>
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Establece un valor en la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="absoluteExpiration">Tiempo de expiración absoluto (opcional)</param>
        /// <param name="slidingExpiration">Tiempo de expiración deslizante (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se estableció correctamente</returns>
        Task<bool> SetAsync<T>(
            string key, 
            T value, 
            TimeSpan? absoluteExpiration = null, 
            TimeSpan? slidingExpiration = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un valor de la caché
        /// </summary>
        /// <param name="key">Clave</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si una clave existe en la caché
        /// </summary>
        /// <param name="key">Clave</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la clave existe</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene o establece un valor en la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="factory">Función para obtener el valor si no existe</param>
        /// <param name="absoluteExpiration">Tiempo de expiración absoluto (opcional)</param>
        /// <param name="slidingExpiration">Tiempo de expiración deslizante (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Valor almacenado o generado</returns>
        Task<T> GetOrSetAsync<T>(
            string key, 
            Func<Task<T>> factory, 
            TimeSpan? absoluteExpiration = null, 
            TimeSpan? slidingExpiration = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Incrementa un contador en la caché
        /// </summary>
        /// <param name="key">Clave</param>
        /// <param name="value">Valor a incrementar (por defecto 1)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Nuevo valor del contador</returns>
        Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Decrementa un contador en la caché
        /// </summary>
        /// <param name="key">Clave</param>
        /// <param name="value">Valor a decrementar (por defecto 1)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Nuevo valor del contador</returns>
        Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza el tiempo de expiración de una clave
        /// </summary>
        /// <param name="key">Clave</param>
        /// <param name="absoluteExpiration">Tiempo de expiración absoluto (opcional)</param>
        /// <param name="slidingExpiration">Tiempo de expiración deslizante (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se actualizó correctamente</returns>
        Task<bool> RefreshExpirationAsync(
            string key, 
            TimeSpan? absoluteExpiration = null, 
            TimeSpan? slidingExpiration = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina todas las claves que coinciden con un patrón
        /// </summary>
        /// <param name="pattern">Patrón de clave (ej: "user:*")</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de claves eliminadas</returns>
        Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Limpia toda la caché
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se limpió correctamente</returns>
        Task<bool> ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el tiempo de expiración de una clave
        /// </summary>
        /// <param name="key">Clave</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tiempo de expiración en segundos o -1 si no existe o no tiene expiración</returns>
        Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);
    }
}
