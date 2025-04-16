using AuthSystem.API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para el middleware de banderas de características
    /// </summary>
    public static class FeatureFlagMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de banderas de características a la pipeline de la aplicación
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseFeatureFlags(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FeatureFlagMiddleware>();
        }
    }
}
