using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Security.Claims;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para enriquecer los logs con información contextual
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Enriquece el contexto de log con información del usuario actual
        /// </summary>
        /// <param name="httpContext">Contexto HTTP</param>
        public static void EnrichLogsWithUserInfo(this HttpContext httpContext)
        {
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                // Usar el scope para añadir propiedades al log
                if (!string.IsNullOrEmpty(userId))
                {
                    Log.Logger.Information("User ID: {UserId}", userId);
                }

                if (!string.IsNullOrEmpty(username))
                {
                    Log.Logger.Information("Username: {Username}", username);
                }

                if (!string.IsNullOrEmpty(userRole))
                {
                    Log.Logger.Information("User Role: {UserRole}", userRole);
                }
            }
        }

        /// <summary>
        /// Enriquece el contexto de log con información de la solicitud HTTP
        /// </summary>
        /// <param name="httpContext">Contexto HTTP</param>
        public static void EnrichLogsWithRequestInfo(this HttpContext httpContext)
        {
            if (httpContext != null)
            {
                // Usar el logger directamente para añadir propiedades al log
                Log.Logger.Information(
                    "Request: {RequestMethod} {RequestPath} {RequestQueryString} {RequestIP} {RequestUserAgent} {RequestHost} {RequestScheme} {RequestProtocol} {RequestContentType} {RequestContentLength}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.Request.QueryString,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers["User-Agent"].ToString(),
                    httpContext.Request.Host.ToString(),
                    httpContext.Request.Scheme,
                    httpContext.Request.Protocol,
                    httpContext.Request.ContentType,
                    httpContext.Request.ContentLength);
            }
        }

        /// <summary>
        /// Registra una excepción con información detallada
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="ex">Excepción</param>
        /// <param name="message">Mensaje</param>
        /// <param name="context">Contexto adicional</param>
        public static void LogDetailedException(this Serilog.ILogger logger, Exception ex, string message, params object[] context)
        {
            // Registrar la excepción con detalles
            logger.Error(
                ex,
                "Exception: {ExceptionType} {ExceptionMessage} {StackTrace} {InnerExceptionType} {InnerExceptionMessage} - {Message}",
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace,
                ex.InnerException?.GetType().Name,
                ex.InnerException?.Message,
                message);
        }
    }
}
