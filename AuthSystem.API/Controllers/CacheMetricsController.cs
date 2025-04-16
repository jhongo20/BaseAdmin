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
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class CacheMetricsController : ControllerBase
    {
        private readonly ICacheMetricsService _cacheMetricsService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheMetricsController> _logger;

        public CacheMetricsController(
            ICacheMetricsService cacheMetricsService,
            ICacheService cacheService,
            ILogger<CacheMetricsController> logger)
        {
            _cacheMetricsService = cacheMetricsService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un resumen de las métricas de caché
        /// </summary>
        /// <returns>Resumen de métricas</returns>
        [HttpGet]
        public IActionResult GetMetrics()
        {
            try
            {
                var metrics = _cacheMetricsService.GetMetricsSummary();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de caché");
                return StatusCode(500, "Error al obtener métricas de caché");
            }
        }

        /// <summary>
        /// Reinicia las métricas de caché
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("reset")]
        public IActionResult ResetMetrics()
        {
            try
            {
                _cacheMetricsService.ResetMetrics();
                return Ok(new { message = "Métricas reiniciadas correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reiniciar métricas de caché");
                return StatusCode(500, "Error al reiniciar métricas de caché");
            }
        }

        /// <summary>
        /// Exporta las métricas a un archivo JSON
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Archivo JSON con las métricas</returns>
        [HttpGet("export")]
        public async Task<IActionResult> ExportMetrics(CancellationToken cancellationToken)
        {
            try
            {
                string tempFilePath = System.IO.Path.GetTempFileName();
                bool success = await _cacheMetricsService.ExportMetricsToJsonAsync(tempFilePath, cancellationToken);
                
                if (!success)
                {
                    return StatusCode(500, "Error al exportar métricas de caché");
                }
                
                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath, cancellationToken);
                System.IO.File.Delete(tempFilePath);
                
                return File(fileBytes, "application/json", $"cache_metrics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar métricas de caché");
                return StatusCode(500, "Error al exportar métricas de caché");
            }
        }

        /// <summary>
        /// Limpia toda la caché
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("clear")]
        public async Task<IActionResult> ClearCache(CancellationToken cancellationToken)
        {
            try
            {
                bool success = await _cacheService.ClearAsync(cancellationToken);
                return Ok(new { success, message = success ? "Caché limpiada correctamente" : "No se pudo limpiar la caché completamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar la caché");
                return StatusCode(500, "Error al limpiar la caché");
            }
        }
    }
}
