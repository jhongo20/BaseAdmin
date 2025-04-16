using AuthSystem.API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para el middleware de revocación de tokens
    /// </summary>
    public static class TokenRevocationMiddlewareExtensions
    {
        /// <summary>
        /// Añade el middleware de revocación de tokens a la pipeline de la aplicación
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseTokenRevocation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenRevocationMiddleware>();
        }
    }
}
