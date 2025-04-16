using AuthSystem.API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para el middleware de validación de sesiones
    /// </summary>
    public static class SessionValidationMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de validación de sesiones a la pipeline de la aplicación
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionValidationMiddleware>();
        }
    }
}
