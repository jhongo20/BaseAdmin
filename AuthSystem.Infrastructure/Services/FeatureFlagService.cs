using System;
using System.Reflection;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de banderas de características (feature flags)
    /// </summary>
    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FeatureFlagService> _logger;
        private FeatureFlags _featureFlags;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <param name="logger">Logger</param>
        public FeatureFlagService(IConfiguration configuration, ILogger<FeatureFlagService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Cargar la configuración inicial
            ReloadFeatures();
        }

        /// <summary>
        /// Obtiene todas las banderas de características
        /// </summary>
        /// <returns>Objeto con todas las banderas de características</returns>
        public FeatureFlags GetAllFeatures()
        {
            return _featureFlags;
        }

        /// <summary>
        /// Verifica si una característica específica está habilitada
        /// </summary>
        /// <param name="featureName">Nombre de la característica</param>
        /// <returns>True si la característica está habilitada, false en caso contrario</returns>
        public bool IsFeatureEnabled(string featureName)
        {
            if (string.IsNullOrEmpty(featureName))
            {
                throw new ArgumentException("El nombre de la característica no puede ser nulo o vacío", nameof(featureName));
            }

            // Obtener la propiedad por su nombre
            PropertyInfo property = typeof(FeatureFlags).GetProperty(featureName);
            if (property == null)
            {
                _logger.LogWarning("La característica {FeatureName} no existe", featureName);
                return false;
            }

            // Obtener el valor de la propiedad
            bool isEnabled = (bool)property.GetValue(_featureFlags);
            return isEnabled;
        }

        /// <summary>
        /// Actualiza el estado de una característica
        /// </summary>
        /// <param name="featureName">Nombre de la característica</param>
        /// <param name="isEnabled">Nuevo estado de la característica</param>
        /// <returns>True si la actualización fue exitosa, false en caso contrario</returns>
        public bool UpdateFeature(string featureName, bool isEnabled)
        {
            if (string.IsNullOrEmpty(featureName))
            {
                throw new ArgumentException("El nombre de la característica no puede ser nulo o vacío", nameof(featureName));
            }

            // Obtener la propiedad por su nombre
            PropertyInfo property = typeof(FeatureFlags).GetProperty(featureName);
            if (property == null)
            {
                _logger.LogWarning("La característica {FeatureName} no existe", featureName);
                return false;
            }

            try
            {
                // Actualizar el valor de la propiedad
                property.SetValue(_featureFlags, isEnabled);
                _logger.LogInformation("Característica {FeatureName} actualizada a {IsEnabled}", featureName, isEnabled);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la característica {FeatureName}", featureName);
                return false;
            }
        }

        /// <summary>
        /// Recarga la configuración de las banderas de características desde la fuente de configuración
        /// </summary>
        public void ReloadFeatures()
        {
            try
            {
                // Cargar la configuración desde appsettings.json
                _featureFlags = _configuration.GetSection("FeatureFlags").Get<FeatureFlags>() ?? new FeatureFlags();
                _logger.LogInformation("Configuración de banderas de características recargada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recargar la configuración de banderas de características");
                _featureFlags = new FeatureFlags();
            }
        }
    }
}
