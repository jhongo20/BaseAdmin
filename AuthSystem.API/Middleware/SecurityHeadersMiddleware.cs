using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AuthSystem.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Agregar encabezados de seguridad
            
            // Content-Security-Policy - Ayuda a prevenir ataques XSS
            context.Response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'");
            
            // X-Content-Type-Options - Evita que el navegador intente adivinar el tipo MIME
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            
            // X-Frame-Options - Protege contra ataques de clickjacking
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            
            // X-XSS-Protection - Activa el filtro XSS en navegadores antiguos
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            
            // Referrer-Policy - Controla qué información de referencia se incluye con las solicitudes
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Strict-Transport-Security - Fuerza conexiones HTTPS
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // Permissions-Policy - Controla qué características y APIs puede usar el sitio
            context.Response.Headers.Add("Permissions-Policy", 
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

            await _next(context);
        }
    }
}
