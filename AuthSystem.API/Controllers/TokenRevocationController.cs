using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de revocación de tokens
    /// </summary>
    [Route("api/token-revocation")]
    [ApiController]
    public class TokenRevocationController : ControllerBase
    {
        private readonly ITokenRevocationService _tokenRevocationService;
        private readonly ILogger<TokenRevocationController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tokenRevocationService">Servicio de revocación de tokens</param>
        /// <param name="logger">Logger</param>
        public TokenRevocationController(
            ITokenRevocationService tokenRevocationService,
            ILogger<TokenRevocationController> logger)
        {
            _tokenRevocationService = tokenRevocationService ?? throw new ArgumentNullException(nameof(tokenRevocationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Revoca el token actual del usuario
        /// </summary>
        /// <param name="reason">Motivo de la revocación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("current")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeCurrentToken([FromQuery] string reason = "Revocado por el usuario")
        {
            try
            {
                // Obtener el ID del usuario actual
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                // Obtener el token actual (sin el prefijo "Bearer ")
                string authHeader = Request.Headers["Authorization"];
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { message = "Token no proporcionado correctamente" });
                }

                string token = authHeader.Substring("Bearer ".Length).Trim();

                // Obtener información del cliente
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string userAgent = Request.Headers["User-Agent"];

                // Revocar el token
                await _tokenRevocationService.RevokeTokenAsync(
                    token,
                    userId,
                    reason,
                    userId, // El usuario que revoca es el mismo usuario
                    ipAddress,
                    userAgent
                );

                return Ok(new { message = "Token revocado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar el token actual");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al revocar el token" });
            }
        }

        /// <summary>
        /// Revoca todos los tokens del usuario actual
        /// </summary>
        /// <param name="reason">Motivo de la revocación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeAllTokens([FromQuery] string reason = "Revocados por el usuario")
        {
            try
            {
                // Obtener el ID del usuario actual
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                // Obtener información del cliente
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string userAgent = Request.Headers["User-Agent"];

                // Revocar todos los tokens del usuario
                int count = await _tokenRevocationService.RevokeAllUserTokensAsync(
                    userId,
                    reason,
                    userId, // El usuario que revoca es el mismo usuario
                    ipAddress,
                    userAgent
                );

                return Ok(new { message = $"{count} tokens revocados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar todos los tokens del usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al revocar los tokens" });
            }
        }

        /// <summary>
        /// Revoca todos los tokens de un usuario específico (solo para administradores)
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Motivo de la revocación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("user/{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeUserTokens(Guid userId, [FromQuery] string reason = "Revocados por un administrador")
        {
            try
            {
                // Obtener el ID del administrador
                if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid adminId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                // Obtener información del cliente
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string userAgent = Request.Headers["User-Agent"];

                // Revocar todos los tokens del usuario
                int count = await _tokenRevocationService.RevokeAllUserTokensAsync(
                    userId,
                    reason,
                    adminId, // El administrador que revoca los tokens
                    ipAddress,
                    userAgent
                );

                return Ok(new { message = $"{count} tokens del usuario {userId} revocados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar los tokens del usuario {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al revocar los tokens" });
            }
        }

        /// <summary>
        /// Limpia los tokens revocados que ya han expirado (solo para administradores)
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CleanupExpiredTokens()
        {
            try
            {
                int count = await _tokenRevocationService.CleanupExpiredTokensAsync();
                return Ok(new { message = $"{count} tokens revocados expirados eliminados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar tokens revocados expirados");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al limpiar tokens revocados" });
            }
        }
    }
}
