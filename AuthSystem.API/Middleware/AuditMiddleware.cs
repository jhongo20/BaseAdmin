using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace AuthSystem.API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly string[] _sensitiveRoutes = new[] 
        { 
            "/api/auth/login", 
            "/api/users", 
            "/api/roles", 
            "/api/permissions" 
        };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            // No auditar rutas de Swagger
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Verificar si la ruta actual es sensible y debe ser auditada
            bool isSensitiveRoute = false;
            foreach (var route in _sensitiveRoutes)
            {
                if (context.Request.Path.StartsWithSegments(route))
                {
                    isSensitiveRoute = true;
                    break;
                }
            }

            // Si no es una ruta sensible, continuar sin auditar
            if (!isSensitiveRoute)
            {
                await _next(context);
                return;
            }

            // Obtener información de la solicitud
            string method = context.Request.Method;
            string path = context.Request.Path;
            string queryString = context.Request.QueryString.ToString();
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string userAgent = context.Request.Headers["User-Agent"].ToString();
            
            // Obtener el ID del usuario si está autenticado
            Guid? userId = null;
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            // Capturar el cuerpo de la solicitud para rutas sensibles
            string requestBody = await GetRequestBodyAsync(context.Request);
            
            // Guardar el cuerpo original para restaurarlo después
            var originalResponseBody = context.Response.Body;
            
            try
            {
                // Crear un nuevo stream para capturar la respuesta
                using var responseBody = _recyclableMemoryStreamManager.GetStream();
                context.Response.Body = responseBody;
                
                // Continuar con la ejecución de la solicitud
                await _next(context);
                
                // Capturar el código de estado de la respuesta
                int statusCode = context.Response.StatusCode;
                
                // Capturar el cuerpo de la respuesta
                string responseBodyText = await GetResponseBodyAsync(context.Response, responseBody);
                
                // Restaurar el cuerpo original
                await responseBody.CopyToAsync(originalResponseBody);
                
                // Registrar la auditoría en la base de datos
                await LogAuditAsync(unitOfWork, method, path, queryString, requestBody, responseBodyText, 
                    statusCode, clientIp, userAgent, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el middleware de auditoría");
                
                // Restaurar el cuerpo original en caso de error
                context.Response.Body = originalResponseBody;
                
                // Continuar con la ejecución
                await _next(context);
            }
            finally
            {
                // Asegurarse de que el cuerpo original se restaure
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            // Habilitar el rebobinado del cuerpo de la solicitud
            request.EnableBuffering();
            
            using var streamReader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            
            var requestBody = await streamReader.ReadToEndAsync();
            
            // Rebobinar el stream para que pueda ser leído nuevamente
            request.Body.Position = 0;
            
            return requestBody;
        }

        private async Task<string> GetResponseBodyAsync(HttpResponse response, MemoryStream responseBody)
        {
            responseBody.Position = 0;
            
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            
            responseBody.Position = 0;
            
            return responseBodyText;
        }

        private async Task LogAuditAsync(
            IUnitOfWork unitOfWork,
            string method,
            string path,
            string queryString,
            string requestBody,
            string responseBody,
            int statusCode,
            string clientIp,
            string userAgent,
            Guid? userId)
        {
            try
            {
                // Sanitizar datos sensibles (por ejemplo, contraseñas)
                requestBody = SanitizeSensitiveData(requestBody);
                responseBody = SanitizeSensitiveData(responseBody);

                // Crear registro de auditoría
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    ActionType = DetermineActionType(method),
                    Endpoint = path,
                    QueryString = queryString,
                    RequestData = requestBody,
                    ResponseData = responseBody,
                    StatusCode = statusCode,
                    IpAddress = clientIp,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                // Guardar en la base de datos
                await unitOfWork.AuditLogs.AddAsync(auditLog);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar registro de auditoría");
            }
        }

        private string SanitizeSensitiveData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            // Sanitizar contraseñas
            data = System.Text.RegularExpressions.Regex.Replace(
                data,
                "\"password\"\\s*:\\s*\"[^\"]*\"",
                "\"password\":\"***\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Sanitizar tokens
            data = System.Text.RegularExpressions.Regex.Replace(
                data,
                "\"token\"\\s*:\\s*\"[^\"]*\"",
                "\"token\":\"***\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            data = System.Text.RegularExpressions.Regex.Replace(
                data,
                "\"refreshToken\"\\s*:\\s*\"[^\"]*\"",
                "\"refreshToken\":\"***\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return data;
        }

        private AuditActionType DetermineActionType(string method)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    return AuditActionType.Read;
                case "POST":
                    return AuditActionType.Create;
                case "PUT":
                    return AuditActionType.Update;
                case "DELETE":
                    return AuditActionType.Delete;
                default:
                    return AuditActionType.Read;
            }
        }
    }
}
