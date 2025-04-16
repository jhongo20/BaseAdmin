using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/feature-flags")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FeatureFlagsController : ControllerBase
    {
        private readonly IFeatureFlagService _featureFlagService;
        private readonly ILogger<FeatureFlagsController> _logger;

        public FeatureFlagsController(
            IFeatureFlagService featureFlagService,
            ILogger<FeatureFlagsController> logger)
        {
            _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene todas las banderas de características
        /// </summary>
        /// <returns>Lista de banderas de características</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllFeatures()
        {
            try
            {
                var features = _featureFlagService.GetAllFeatures();
                
                // Convertir a un diccionario para la respuesta
                var response = new Dictionary<string, bool>();
                foreach (var property in typeof(FeatureFlags).GetProperties())
                {
                    response[property.Name] = (bool)property.GetValue(features);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener banderas de características");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener banderas de características" });
            }
        }

        /// <summary>
        /// Verifica si una característica específica está habilitada
        /// </summary>
        /// <param name="featureName">Nombre de la característica</param>
        /// <returns>Estado de la característica</returns>
        [HttpGet("{featureName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFeature(string featureName)
        {
            try
            {
                // Verificar si la característica existe
                PropertyInfo property = typeof(FeatureFlags).GetProperty(featureName);
                if (property == null)
                {
                    return NotFound(new { message = $"La característica '{featureName}' no existe" });
                }
                
                bool isEnabled = _featureFlagService.IsFeatureEnabled(featureName);
                return Ok(new { featureName, isEnabled });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la característica {FeatureName}", featureName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener la característica" });
            }
        }

        /// <summary>
        /// Actualiza el estado de una característica
        /// </summary>
        /// <param name="featureName">Nombre de la característica</param>
        /// <param name="request">Datos de la actualización</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("{featureName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateFeature(string featureName, [FromBody] UpdateFeatureRequest request)
        {
            try
            {
                // Verificar si la característica existe
                PropertyInfo property = typeof(FeatureFlags).GetProperty(featureName);
                if (property == null)
                {
                    return NotFound(new { message = $"La característica '{featureName}' no existe" });
                }
                
                bool result = _featureFlagService.UpdateFeature(featureName, request.IsEnabled);
                if (result)
                {
                    return Ok(new { message = $"Característica '{featureName}' actualizada correctamente", isEnabled = request.IsEnabled });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo actualizar la característica" });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la característica {FeatureName}", featureName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al actualizar la característica" });
            }
        }

        /// <summary>
        /// Recarga la configuración de las banderas de características
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("reload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ReloadFeatures()
        {
            try
            {
                _featureFlagService.ReloadFeatures();
                return Ok(new { message = "Configuración de banderas de características recargada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recargar la configuración de banderas de características");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al recargar la configuración" });
            }
        }
    }

    /// <summary>
    /// Modelo para la solicitud de actualización de una característica
    /// </summary>
    public class UpdateFeatureRequest
    {
        /// <summary>
        /// Nuevo estado de la característica
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
