using AuthSystem.API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para el middleware de logging
    /// </summary>
    public static class LoggingMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de logging a la pipeline de la aplicación
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
