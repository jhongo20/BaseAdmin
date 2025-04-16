using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Middleware
{
    /// <summary>
    /// Middleware para validar y actualizar las sesiones en cada solicitud
    /// </summary>
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Siguiente middleware en la pipeline</param>
        /// <param name="logger">Logger</param>
        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Método de invocación del middleware
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <param name="sessionManagementService">Servicio de gestión de sesiones</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task InvokeAsync(HttpContext context, ISessionManagementService sessionManagementService)
        {
            try
            {
                // Verificar si hay un token de autorización
                string authHeader = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    // Extraer el token
                    string token = authHeader.Substring("Bearer ".Length).Trim();

                    // Extraer el JTI del token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    if (tokenHandler.CanReadToken(token))
                    {
                        var jwtToken = tokenHandler.ReadJwtToken(token);
                        string tokenId = jwtToken.Id;

                        // Verificar si la sesión existe y está activa
                        var session = await sessionManagementService.GetSessionByTokenIdAsync(tokenId);
                        if (session != null && session.IsActive && session.EndedAt == null)
                        {
                            // Actualizar la actividad de la sesión
                            await sessionManagementService.UpdateSessionActivityAsync(session.Id);
                        }
                        else if (session == null)
                        {
                            _logger.LogWarning("Intento de uso de un token sin sesión asociada");
                        }
                        else if (!session.IsActive || session.EndedAt != null)
                        {
                            _logger.LogWarning("Intento de uso de un token con sesión cerrada");
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(new { message = "Sesión cerrada. Por favor, inicie sesión nuevamente." });
                            return;
                        }
                    }
                }

                // Continuar con el siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el middleware de validación de sesiones");
                await _next(context);
            }
        }
    }
}
