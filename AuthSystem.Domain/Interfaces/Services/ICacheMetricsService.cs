using AuthSystem.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de métricas de caché
    /// </summary>
    public interface ICacheMetricsService
    {
        /// <summary>
        /// Registra un acierto de caché (cache hit)
        /// </summary>
        /// <param name="key">Clave de caché</param>
        /// <param name="responseTimeMs">Tiempo de respuesta en milisegundos</param>
        void RecordHit(string key, long responseTimeMs);
        
        /// <summary>
        /// Registra un fallo de caché (cache miss)
        /// </summary>
        /// <param name="key">Clave de caché</param>
        /// <param name="responseTimeMs">Tiempo de respuesta en milisegundos</param>
        void RecordMiss(string key, long responseTimeMs);
        
        /// <summary>
        /// Registra una expiración de caché
        /// </summary>
        /// <param name="key">Clave de caché</param>
        void RecordExpiration(string key);
        
        /// <summary>
        /// Registra una eliminación manual de caché
        /// </summary>
        /// <param name="key">Clave de caché</param>
        void RecordEviction(string key);
        
        /// <summary>
        /// Actualiza el contador de elementos en caché
        /// </summary>
        /// <param name="count">Número de elementos</param>
        void UpdateItemCount(long count);
        
        /// <summary>
        /// Obtiene todas las métricas de caché
        /// </summary>
        /// <returns>Métricas de caché</returns>
        CacheMetrics GetMetrics();
        
        /// <summary>
        /// Obtiene un resumen de las métricas de caché
        /// </summary>
        /// <returns>Resumen de métricas</returns>
        Dictionary<string, object> GetMetricsSummary();
        
        /// <summary>
        /// Reinicia las métricas de caché
        /// </summary>
        void ResetMetrics();
        
        /// <summary>
        /// Exporta las métricas a un archivo JSON
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si se exportó correctamente</returns>
        Task<bool> ExportMetricsToJsonAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
