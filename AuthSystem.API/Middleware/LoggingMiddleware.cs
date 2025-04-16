using AuthSystem.API.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AuthSystem.API.Middleware
{
    /// <summary>
    /// Middleware para enriquecer los logs con información contextual
    /// </summary>
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Siguiente middleware en la pipeline</param>
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = Serilog.Log.ForContext<LoggingMiddleware>();
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Método de invocación del middleware
        /// </summary>
        /// <param name="context">Contexto HTTP</param>
        /// <returns>Tarea asíncrona</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Generar un ID de correlación para la solicitud
            var correlationId = Guid.NewGuid().ToString();
            context.Response.Headers.Append("X-Correlation-ID", correlationId);

            try
            {
                // Enriquecer logs con información de la solicitud
                context.EnrichLogsWithRequestInfo();
                context.EnrichLogsWithUserInfo();

                // Registrar inicio de la solicitud con el ID de correlación
                _stopwatch.Start();
                _logger.Information("Solicitud iniciada: {Method} {Path} {CorrelationId}", 
                    context.Request.Method, 
                    context.Request.Path,
                    correlationId);

                // Continuar con el siguiente middleware
                await _next(context);

                // Registrar finalización de la solicitud
                _stopwatch.Stop();
                _logger.Information("Solicitud completada: {Method} {Path} - Estado: {StatusCode} - Duración: {ElapsedMilliseconds}ms {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    _stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
            catch (Exception ex)
            {
                // Registrar excepción
                _stopwatch.Stop();
                _logger.Error(ex, "Error no controlado: {Method} {Path} - Duración: {ElapsedMilliseconds}ms {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    _stopwatch.ElapsedMilliseconds,
                    correlationId);

                // Re-lanzar la excepción para que sea manejada por el middleware de excepciones
                throw;
            }
        }
    }
}
