using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthSystem.API.Controllers
{
    /// <summary>
    /// Controlador para probar el rate limiting
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimitTestController : ControllerBase
    {
        private readonly ILogger<RateLimitTestController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        public RateLimitTestController(ILogger<RateLimitTestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Endpoint para probar el rate limiting
        /// </summary>
        /// <returns>Respuesta de prueba</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Rate limit test endpoint called at {Time}", DateTime.UtcNow);
            
            // Obtener información de los headers de rate limiting
            var headers = new Dictionary<string, string>();
            if (Response.Headers.TryGetValue("X-RateLimit-Limit", out var limit))
            {
                headers.Add("X-RateLimit-Limit", limit.ToString());
            }
            
            if (Response.Headers.TryGetValue("X-RateLimit-Remaining", out var remaining))
            {
                headers.Add("X-RateLimit-Remaining", remaining.ToString());
            }
            
            if (Response.Headers.TryGetValue("X-RateLimit-Reset", out var reset))
            {
                headers.Add("X-RateLimit-Reset", reset.ToString());
            }

            return Ok(new
            {
                message = "Rate limit test successful",
                timestamp = DateTime.UtcNow,
                rateLimitHeaders = headers
            });
        }

        /// <summary>
        /// Endpoint para probar el rate limiting con un cliente específico
        /// </summary>
        /// <returns>Respuesta de prueba</returns>
        [HttpGet("client")]
        public IActionResult GetWithClientId()
        {
            var clientId = Request.Headers.TryGetValue("X-ClientId", out var clientIdValue)
                ? clientIdValue.ToString()
                : "unknown";

            _logger.LogInformation("Rate limit test endpoint called with ClientId: {ClientId} at {Time}", 
                clientId, DateTime.UtcNow);
            
            return Ok(new
            {
                message = "Rate limit test with client ID successful",
                clientId = clientId,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
