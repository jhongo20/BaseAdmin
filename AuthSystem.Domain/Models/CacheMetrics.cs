using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models
{
    /// <summary>
    /// Clase para almacenar métricas de caché
    /// </summary>
    public class CacheMetrics
    {
        /// <summary>
        /// Número total de solicitudes de caché
        /// </summary>
        public long TotalRequests { get; set; }
        
        /// <summary>
        /// Número de aciertos de caché (cache hits)
        /// </summary>
        public long Hits { get; set; }
        
        /// <summary>
        /// Número de fallos de caché (cache misses)
        /// </summary>
        public long Misses { get; set; }
        
        /// <summary>
        /// Tiempo total de respuesta para aciertos de caché (en milisegundos)
        /// </summary>
        public long TotalHitResponseTime { get; set; }
        
        /// <summary>
        /// Tiempo total de respuesta para fallos de caché (en milisegundos)
        /// </summary>
        public long TotalMissResponseTime { get; set; }
        
        /// <summary>
        /// Número de elementos actualmente en caché
        /// </summary>
        public long CurrentItems { get; set; }
        
        /// <summary>
        /// Número de expiraciones de caché
        /// </summary>
        public long Expirations { get; set; }
        
        /// <summary>
        /// Número de eliminaciones manuales de caché
        /// </summary>
        public long Evictions { get; set; }
        
        /// <summary>
        /// Métricas por tipo de clave (por ejemplo, ldap, configuración, etc.)
        /// </summary>
        public ConcurrentDictionary<string, TypeMetrics> MetricsByType { get; set; } = new ConcurrentDictionary<string, TypeMetrics>();
        
        /// <summary>
        /// Fecha y hora de inicio del monitoreo
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Fecha y hora de la última actualización
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Obtiene la tasa de aciertos de caché (hit rate)
        /// </summary>
        public double HitRate => TotalRequests > 0 ? (double)Hits / TotalRequests * 100 : 0;
        
        /// <summary>
        /// Obtiene el tiempo promedio de respuesta para aciertos de caché (en milisegundos)
        /// </summary>
        public double AverageHitResponseTime => Hits > 0 ? (double)TotalHitResponseTime / Hits : 0;
        
        /// <summary>
        /// Obtiene el tiempo promedio de respuesta para fallos de caché (en milisegundos)
        /// </summary>
        public double AverageMissResponseTime => Misses > 0 ? (double)TotalMissResponseTime / Misses : 0;
        
        /// <summary>
        /// Obtiene un resumen de las métricas
        /// </summary>
        public Dictionary<string, object> GetSummary()
        {
            return new Dictionary<string, object>
            {
                { "TotalRequests", TotalRequests },
                { "Hits", Hits },
                { "Misses", Misses },
                { "HitRate", HitRate },
                { "AverageHitResponseTime", AverageHitResponseTime },
                { "AverageMissResponseTime", AverageMissResponseTime },
                { "CurrentItems", CurrentItems },
                { "Expirations", Expirations },
                { "Evictions", Evictions },
                { "StartTime", StartTime },
                { "LastUpdateTime", LastUpdateTime },
                { "UptimeSeconds", (DateTime.UtcNow - StartTime).TotalSeconds }
            };
        }
    }
    
    /// <summary>
    /// Métricas para un tipo específico de clave de caché
    /// </summary>
    public class TypeMetrics
    {
        /// <summary>
        /// Número total de solicitudes para este tipo
        /// </summary>
        public long Requests { get; set; }
        
        /// <summary>
        /// Número de aciertos para este tipo
        /// </summary>
        public long Hits { get; set; }
        
        /// <summary>
        /// Número de fallos para este tipo
        /// </summary>
        public long Misses { get; set; }
        
        /// <summary>
        /// Tiempo total de respuesta para aciertos (en milisegundos)
        /// </summary>
        public long TotalHitResponseTime { get; set; }
        
        /// <summary>
        /// Tiempo total de respuesta para fallos (en milisegundos)
        /// </summary>
        public long TotalMissResponseTime { get; set; }
        
        /// <summary>
        /// Obtiene la tasa de aciertos para este tipo
        /// </summary>
        public double HitRate => Requests > 0 ? (double)Hits / Requests * 100 : 0;
        
        /// <summary>
        /// Obtiene el tiempo promedio de respuesta para aciertos (en milisegundos)
        /// </summary>
        public double AverageHitResponseTime => Hits > 0 ? (double)TotalHitResponseTime / Hits : 0;
        
        /// <summary>
        /// Obtiene el tiempo promedio de respuesta para fallos (en milisegundos)
        /// </summary>
        public double AverageMissResponseTime => Misses > 0 ? (double)TotalMissResponseTime / Misses : 0;
    }
}
