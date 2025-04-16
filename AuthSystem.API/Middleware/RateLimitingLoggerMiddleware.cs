using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using AspNetCoreRateLimit;

namespace AuthSystem.API.Middleware
{
    /// <summary>
    /// Middleware para registrar los intentos de rate limiting
    /// </summary>
    public class RateLimitingLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingLoggerMiddleware> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Siguiente middleware en la cadena</param>
        /// <param name="logger">Logger</param>
        public RateLimitingLoggerMiddleware(RequestDelegate next, ILogger<RateLimitingLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Método de invocación del middleware
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Verificar si se ha excedido el límite de solicitudes
            if (context.Response.Headers.TryGetValue("X-RateLimit-Limit", out var limit) &&
                context.Response.Headers.TryGetValue("X-RateLimit-Remaining", out var remaining))
            {
                if (int.TryParse(remaining.ToString(), out int remainingValue) && remainingValue <= 0)
                {
                    var clientId = context.Request.Headers.TryGetValue("X-ClientId", out var clientIdValue)
                        ? clientIdValue.ToString()
                        : "unknown";

                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var path = context.Request.Path.Value ?? "unknown";
                    var method = context.Request.Method;

                    _logger.LogWarning(
                        "Rate limit exceeded: IP={IpAddress}, ClientId={ClientId}, Path={Path}, Method={Method}, Limit={Limit}",
                        ipAddress, clientId, path, method, limit);

                    // Aquí se podría implementar lógica adicional como:
                    // - Registrar en base de datos para análisis posterior
                    // - Enviar alertas si se detecta un patrón de abuso
                    // - Bloquear temporalmente IPs o clientes que abusan constantemente
                }
            }

            await _next(context);
        }
    }
}
