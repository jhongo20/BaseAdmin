using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/security-monitoring")]
    [Authorize(Roles = "Admin")]
    public class SecurityMonitoringController : ControllerBase
    {
        private readonly ISecurityMonitoringService _securityMonitoringService;
        private readonly ILogger<SecurityMonitoringController> _logger;

        public SecurityMonitoringController(
            ISecurityMonitoringService securityMonitoringService,
            ILogger<SecurityMonitoringController> logger)
        {
            _securityMonitoringService = securityMonitoringService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las alertas de seguridad recientes
        /// </summary>
        /// <param name="count">Número máximo de alertas a obtener</param>
        /// <returns>Lista de alertas de seguridad</returns>
        [HttpGet("alerts")]
        public async Task<ActionResult<IEnumerable<SecurityAlert>>> GetRecentAlerts([FromQuery] int count = 10)
        {
            try
            {
                var alerts = await _securityMonitoringService.GetRecentAlertsAsync(count);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas de seguridad recientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las estadísticas de seguridad
        /// </summary>
        /// <returns>Estadísticas de seguridad</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<SecurityStats>> GetSecurityStats()
        {
            try
            {
                var stats = await _securityMonitoringService.GetSecurityStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de seguridad");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Marca una alerta como revisada
        /// </summary>
        /// <param name="id">Identificador de la alerta</param>
        /// <param name="notes">Notas sobre la revisión</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("alerts/{id}/review")]
        public async Task<IActionResult> ReviewAlert(Guid id, [FromBody] string notes)
        {
            try
            {
                // En una implementación real, aquí se marcaría la alerta como revisada en la base de datos
                _logger.LogInformation("Alerta {AlertId} marcada como revisada por {Username} con notas: {Notes}", 
                    id, User.Identity.Name, notes);
                
                return Ok(new { message = "Alerta marcada como revisada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar alerta como revisada: {AlertId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Ejecuta manualmente la detección de patrones sospechosos
        /// </summary>
        /// <returns>Alertas generadas</returns>
        [HttpPost("detect")]
        public async Task<ActionResult<IEnumerable<SecurityAlert>>> DetectSuspiciousPatterns()
        {
            try
            {
                var alerts = await _securityMonitoringService.DetectSuspiciousPatterns();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar detección de patrones sospechosos");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
