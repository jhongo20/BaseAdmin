using System;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Middleware
{
    /// <summary>
    /// Middleware para verificar si las características requeridas están habilitadas
    /// </summary>
    public class FeatureFlagMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FeatureFlagMiddleware> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Siguiente middleware en la pipeline</param>
        /// <param name="logger">Logger</param>
        public FeatureFlagMiddleware(RequestDelegate next, ILogger<FeatureFlagMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Método de invocación del middleware
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <param name="featureFlagService">Servicio de banderas de características</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task InvokeAsync(HttpContext context, IFeatureFlagService featureFlagService)
        {
            try
            {
                // Verificar si la ruta contiene "api/v"
                string path = context.Request.Path.Value?.ToLower() ?? string.Empty;
                
                // Verificar la API versionada
                if (path.Contains("/api/v") && !featureFlagService.IsFeatureEnabled("EnableVersionedApi"))
                {
                    _logger.LogWarning("Intento de acceso a API versionada cuando está deshabilitada");
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { message = "La API versionada está deshabilitada" });
                    return;
                }
                
                // Verificar la revocación de tokens
                if (path.Contains("/api/v1/token-revocation") && !featureFlagService.IsFeatureEnabled("EnableTokenRevocation"))
                {
                    _logger.LogWarning("Intento de acceso a revocación de tokens cuando está deshabilitada");
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { message = "La revocación de tokens está deshabilitada" });
                    return;
                }
                
                // Verificar las sesiones distribuidas
                if (path.Contains("/api/v1/sessions") && !featureFlagService.IsFeatureEnabled("EnableDistributedSessions"))
                {
                    _logger.LogWarning("Intento de acceso a sesiones distribuidas cuando está deshabilitada");
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { message = "Las sesiones distribuidas están deshabilitadas" });
                    return;
                }
                
                // Verificar Swagger
                if (path.Contains("/swagger") && !featureFlagService.IsFeatureEnabled("EnableSwagger"))
                {
                    _logger.LogWarning("Intento de acceso a Swagger cuando está deshabilitado");
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new { message = "Swagger está deshabilitado" });
                    return;
                }

                // Continuar con el siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el middleware de banderas de características");
                await _next(context);
            }
        }
    }
}
