using AuthSystem.Domain.Models;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de banderas de características (feature flags)
    /// </summary>
    public interface IFeatureFlagService
    {
        /// <summary>
        /// Obtiene todas las banderas de características
        /// </summary>
        /// <returns>Objeto con todas las banderas de características</returns>
        FeatureFlags GetAllFeatures();

        /// <summary>
        /// Verifica si una característica específica está habilitada
        /// </summary>
        /// <param name="featureName">Nombre de la característica</param>
        /// <returns>True si la característica está habilitada, false en caso contrario</returns>
        bool IsFeatureEnabled(string featureName);

        /// <summary>
        /// Actualiza el estado de una característica
        /// </summary>
        /// <param name="featureName">Nombre de la característica</param>
        /// <param name="isEnabled">Nuevo estado de la característica</param>
        /// <returns>True si la actualización fue exitosa, false en caso contrario</returns>
        bool UpdateFeature(string featureName, bool isEnabled);

        /// <summary>
        /// Recarga la configuración de las banderas de características desde la fuente de configuración
        /// </summary>
        void ReloadFeatures();
    }
}
