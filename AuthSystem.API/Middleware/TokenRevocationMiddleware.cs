using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Middleware
{
    /// <summary>
    /// Middleware para verificar si los tokens están revocados
    /// </summary>
    public class TokenRevocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenRevocationMiddleware> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Siguiente middleware en la pipeline</param>
        /// <param name="logger">Logger</param>
        public TokenRevocationMiddleware(RequestDelegate next, ILogger<TokenRevocationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Método de invocación del middleware
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <param name="tokenRevocationService">Servicio de revocación de tokens</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task InvokeAsync(HttpContext context, ITokenRevocationService tokenRevocationService)
        {
            try
            {
                // Verificar si hay un token de autorización
                string authHeader = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    // Extraer el token
                    string token = authHeader.Substring("Bearer ".Length).Trim();

                    // Verificar si el token está revocado
                    bool isRevoked = await tokenRevocationService.IsTokenRevokedAsync(token);
                    if (isRevoked)
                    {
                        _logger.LogWarning("Intento de uso de un token revocado");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = "Token revocado. Por favor, inicie sesión nuevamente." });
                        return;
                    }

                    // Verificar si el JTI está revocado
                    try
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(token);
                        string tokenId = jwtToken.Id;

                        if (!string.IsNullOrEmpty(tokenId))
                        {
                            bool isTokenIdRevoked = await tokenRevocationService.IsTokenIdRevokedAsync(tokenId);
                            if (isTokenIdRevoked)
                            {
                                _logger.LogWarning("Intento de uso de un token con JTI revocado");
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsJsonAsync(new { message = "Token revocado. Por favor, inicie sesión nuevamente." });
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al verificar el JTI del token");
                        // Continuamos con la ejecución normal, ya que esto no debería bloquear la solicitud
                    }
                }

                // Continuar con el siguiente middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el middleware de revocación de tokens");
                await _next(context);
            }
        }
    }
}
