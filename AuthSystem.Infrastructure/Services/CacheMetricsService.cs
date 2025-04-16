using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de métricas de caché
    /// </summary>
    public class CacheMetricsService : ICacheMetricsService
    {
        private readonly ILogger<CacheMetricsService> _logger;
        private readonly CacheMetrics _metrics = new CacheMetrics();
        private readonly object _lock = new object();

        public CacheMetricsService(ILogger<CacheMetricsService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void RecordHit(string key, long responseTimeMs)
        {
            try
            {
                lock (_lock)
                {
                    _metrics.TotalRequests++;
                    _metrics.Hits++;
                    _metrics.TotalHitResponseTime += responseTimeMs;
                    _metrics.LastUpdateTime = DateTime.UtcNow;

                    // Extraer el tipo de la clave (por ejemplo, "ldap:config" -> "ldap")
                    string keyType = GetKeyType(key);
                    var typeMetrics = _metrics.MetricsByType.GetOrAdd(keyType, _ => new TypeMetrics());
                    typeMetrics.Requests++;
                    typeMetrics.Hits++;
                    typeMetrics.TotalHitResponseTime += responseTimeMs;
                }

                _logger.LogTrace("Cache hit recorded for key: {Key}, response time: {ResponseTime}ms", key, responseTimeMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache hit for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public void RecordMiss(string key, long responseTimeMs)
        {
            try
            {
                lock (_lock)
                {
                    _metrics.TotalRequests++;
                    _metrics.Misses++;
                    _metrics.TotalMissResponseTime += responseTimeMs;
                    _metrics.LastUpdateTime = DateTime.UtcNow;

                    // Extraer el tipo de la clave
                    string keyType = GetKeyType(key);
                    var typeMetrics = _metrics.MetricsByType.GetOrAdd(keyType, _ => new TypeMetrics());
                    typeMetrics.Requests++;
                    typeMetrics.Misses++;
                    typeMetrics.TotalMissResponseTime += responseTimeMs;
                }

                _logger.LogTrace("Cache miss recorded for key: {Key}, response time: {ResponseTime}ms", key, responseTimeMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache miss for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public void RecordExpiration(string key)
        {
            try
            {
                lock (_lock)
                {
                    _metrics.Expirations++;
                    _metrics.LastUpdateTime = DateTime.UtcNow;
                }

                _logger.LogTrace("Cache expiration recorded for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache expiration for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public void RecordEviction(string key)
        {
            try
            {
                lock (_lock)
                {
                    _metrics.Evictions++;
                    _metrics.LastUpdateTime = DateTime.UtcNow;
                }

                _logger.LogTrace("Cache eviction recorded for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache eviction for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public void UpdateItemCount(long count)
        {
            try
            {
                lock (_lock)
                {
                    _metrics.CurrentItems = count;
                    _metrics.LastUpdateTime = DateTime.UtcNow;
                }

                _logger.LogTrace("Cache item count updated: {Count}", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cache item count");
            }
        }

        /// <inheritdoc />
        public CacheMetrics GetMetrics()
        {
            lock (_lock)
            {
                // Crear una copia para evitar problemas de concurrencia
                var metricsCopy = new CacheMetrics
                {
                    TotalRequests = _metrics.TotalRequests,
                    Hits = _metrics.Hits,
                    Misses = _metrics.Misses,
                    TotalHitResponseTime = _metrics.TotalHitResponseTime,
                    TotalMissResponseTime = _metrics.TotalMissResponseTime,
                    CurrentItems = _metrics.CurrentItems,
                    Expirations = _metrics.Expirations,
                    Evictions = _metrics.Evictions,
                    StartTime = _metrics.StartTime,
                    LastUpdateTime = _metrics.LastUpdateTime
                };

                // Copiar las métricas por tipo
                foreach (var kvp in _metrics.MetricsByType)
                {
                    var typeMetricsCopy = new TypeMetrics
                    {
                        Requests = kvp.Value.Requests,
                        Hits = kvp.Value.Hits,
                        Misses = kvp.Value.Misses,
                        TotalHitResponseTime = kvp.Value.TotalHitResponseTime,
                        TotalMissResponseTime = kvp.Value.TotalMissResponseTime
                    };
                    metricsCopy.MetricsByType.TryAdd(kvp.Key, typeMetricsCopy);
                }

                return metricsCopy;
            }
        }

        /// <inheritdoc />
        public Dictionary<string, object> GetMetricsSummary()
        {
            lock (_lock)
            {
                var summary = _metrics.GetSummary();
                
                // Añadir resumen por tipo
                var typesSummary = new Dictionary<string, object>();
                foreach (var kvp in _metrics.MetricsByType)
                {
                    typesSummary[kvp.Key] = new Dictionary<string, object>
                    {
                        { "Requests", kvp.Value.Requests },
                        { "Hits", kvp.Value.Hits },
                        { "Misses", kvp.Value.Misses },
                        { "HitRate", kvp.Value.HitRate },
                        { "AverageHitResponseTime", kvp.Value.AverageHitResponseTime },
                        { "AverageMissResponseTime", kvp.Value.AverageMissResponseTime }
                    };
                }
                summary["TypeMetrics"] = typesSummary;
                
                return summary;
            }
        }

        /// <inheritdoc />
        public void ResetMetrics()
        {
            lock (_lock)
            {
                _metrics.TotalRequests = 0;
                _metrics.Hits = 0;
                _metrics.Misses = 0;
                _metrics.TotalHitResponseTime = 0;
                _metrics.TotalMissResponseTime = 0;
                _metrics.Expirations = 0;
                _metrics.Evictions = 0;
                _metrics.StartTime = DateTime.UtcNow;
                _metrics.LastUpdateTime = DateTime.UtcNow;
                _metrics.MetricsByType.Clear();
            }

            _logger.LogInformation("Cache metrics reset");
        }

        /// <inheritdoc />
        public async Task<bool> ExportMetricsToJsonAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var metrics = GetMetrics();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(metrics, options);
                
                await File.WriteAllTextAsync(filePath, json, cancellationToken);
                
                _logger.LogInformation("Cache metrics exported to: {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting cache metrics to: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Extrae el tipo de una clave de caché
        /// </summary>
        /// <param name="key">Clave de caché</param>
        /// <returns>Tipo de clave</returns>
        private string GetKeyType(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "unknown";

            // Extraer el tipo de la clave (por ejemplo, "ldap:config:123" -> "ldap")
            var parts = key.Split(':');
            return parts.Length > 0 ? parts[0] : "unknown";
        }
    }
}
