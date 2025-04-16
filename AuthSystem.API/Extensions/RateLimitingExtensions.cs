using Microsoft.AspNetCore.Builder;
using AuthSystem.API.Middleware;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para configurar el rate limiting
    /// </summary>
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Agrega el middleware de registro de rate limiting
        /// </summary>
        /// <param name="app">Aplicación</param>
        /// <returns>Aplicación con el middleware configurado</returns>
        public static IApplicationBuilder UseRateLimitingLogger(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingLoggerMiddleware>();
        }
    }
}
