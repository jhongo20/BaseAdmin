using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/sessions")]
    [ApiController]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionManagementService _sessionManagementService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(
            ISessionManagementService sessionManagementService,
            ILogger<SessionsController> logger)
        {
            _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene todas las sesiones activas del usuario actual
        /// </summary>
        /// <returns>Lista de sesiones activas</returns>
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyActiveSessions()
        {
            try
            {
                // Obtener el ID del usuario actual
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                var sessions = await _sessionManagementService.GetActiveSessionsByUserIdAsync(userId);
                
                var result = new List<object>();
                foreach (var session in sessions)
                {
                    result.Add(new
                    {
                        Id = session.Id,
                        CreatedAt = session.CreatedAt,
                        LastActivity = session.LastActivity,
                        ExpiresAt = session.ExpiresAt,
                        IpAddress = session.IpAddress,
                        UserAgent = session.UserAgent,
                        IsCurrentSession = IsCurrentSession(session.TokenId)
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones activas del usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener sesiones activas" });
            }
        }

        /// <summary>
        /// Cierra una sesión específica del usuario actual
        /// </summary>
        /// <param name="sessionId">ID de la sesión a cerrar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("my/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseMySession(Guid sessionId)
        {
            try
            {
                // Obtener el ID del usuario actual
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                // Verificar si la sesión pertenece al usuario
                var session = await _sessionManagementService.GetSessionByIdAsync(sessionId);
                if (session == null)
                {
                    return NotFound(new { message = "Sesión no encontrada" });
                }

                if (session.UserId != userId)
                {
                    return Forbid();
                }

                // Verificar si es la sesión actual
                if (IsCurrentSession(session.TokenId))
                {
                    return BadRequest(new { message = "No se puede cerrar la sesión actual. Utilice el endpoint de cierre de sesión" });
                }

                // Cerrar la sesión
                bool result = await _sessionManagementService.CloseSessionAsync(sessionId, "Cerrada por el usuario");
                if (result)
                {
                    return Ok(new { message = "Sesión cerrada correctamente" });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo cerrar la sesión" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión {SessionId}", sessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al cerrar sesión" });
            }
        }

        /// <summary>
        /// Cierra todas las sesiones del usuario actual excepto la sesión actual
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseAllMyOtherSessions()
        {
            try
            {
                // Obtener el ID del usuario actual
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }

                // Obtener el ID de la sesión actual
                string tokenId = User.FindFirst("jti")?.Value;
                if (string.IsNullOrEmpty(tokenId))
                {
                    return Unauthorized(new { message = "Token no válido" });
                }

                var currentSession = await _sessionManagementService.GetSessionByTokenIdAsync(tokenId);
                if (currentSession == null)
                {
                    return BadRequest(new { message = "No se encontró la sesión actual" });
                }

                // Cerrar todas las sesiones excepto la actual
                int closedCount = await _sessionManagementService.CloseAllOtherSessionsAsync(
                    userId, 
                    currentSession.Id, 
                    "Cerradas por el usuario"
                );

                return Ok(new { message = $"Se cerraron {closedCount} sesiones correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar todas las sesiones excepto la actual");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al cerrar sesiones" });
            }
        }

        /// <summary>
        /// Obtiene todas las sesiones activas (solo para administradores)
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de sesiones activas</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllActiveSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var (sessions, totalCount) = await _sessionManagementService.GetAllActiveSessionsAsync(page, pageSize);
                
                var result = new List<object>();
                foreach (var session in sessions)
                {
                    result.Add(new
                    {
                        Id = session.Id,
                        UserId = session.UserId,
                        CreatedAt = session.CreatedAt,
                        LastActivity = session.LastActivity,
                        ExpiresAt = session.ExpiresAt,
                        IpAddress = session.IpAddress,
                        UserAgent = session.UserAgent
                    });
                }

                return Ok(new
                {
                    Sessions = result,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las sesiones activas");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener sesiones activas" });
            }
        }

        /// <summary>
        /// Cierra una sesión específica (solo para administradores)
        /// </summary>
        /// <param name="sessionId">ID de la sesión a cerrar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{sessionId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseSession(Guid sessionId)
        {
            try
            {
                // Verificar si la sesión existe
                var session = await _sessionManagementService.GetSessionByIdAsync(sessionId);
                if (session == null)
                {
                    return NotFound(new { message = "Sesión no encontrada" });
                }

                // Verificar si es la sesión actual
                if (IsCurrentSession(session.TokenId))
                {
                    return BadRequest(new { message = "No se puede cerrar la sesión actual. Utilice el endpoint de cierre de sesión" });
                }

                // Cerrar la sesión
                bool result = await _sessionManagementService.CloseSessionAsync(sessionId, "Cerrada por un administrador");
                if (result)
                {
                    return Ok(new { message = "Sesión cerrada correctamente" });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo cerrar la sesión" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión {SessionId}", sessionId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al cerrar sesión" });
            }
        }

        /// <summary>
        /// Cierra todas las sesiones de un usuario específico (solo para administradores)
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("user/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseAllUserSessions(Guid userId)
        {
            try
            {
                // Cerrar todas las sesiones del usuario
                int closedCount = await _sessionManagementService.CloseAllUserSessionsAsync(
                    userId, 
                    "Cerradas por un administrador"
                );

                return Ok(new { message = $"Se cerraron {closedCount} sesiones correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar todas las sesiones del usuario {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al cerrar sesiones" });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de sesiones (solo para administradores)
        /// </summary>
        /// <returns>Estadísticas de sesiones</returns>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSessionStats()
        {
            try
            {
                var totalActiveSessions = await _sessionManagementService.GetTotalActiveSessionsAsync();
                var sessionsByRole = await _sessionManagementService.GetActiveSessionsByRoleAsync();
                var newSessionsLast24Hours = await _sessionManagementService.GetNewSessionsInLastHoursAsync(24);

                return Ok(new
                {
                    TotalActiveSessions = totalActiveSessions,
                    SessionsByRole = sessionsByRole,
                    NewSessionsLast24Hours = newSessionsLast24Hours
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de sesiones");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener estadísticas de sesiones" });
            }
        }

        /// <summary>
        /// Limpia las sesiones expiradas (solo para administradores)
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("cleanup")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CleanupExpiredSessions()
        {
            try
            {
                int cleanedCount = await _sessionManagementService.CleanupExpiredSessionsAsync();
                return Ok(new { message = $"Se limpiaron {cleanedCount} sesiones expiradas" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar sesiones expiradas");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al limpiar sesiones expiradas" });
            }
        }

        /// <summary>
        /// Verifica si la sesión actual corresponde al token ID especificado
        /// </summary>
        /// <param name="tokenId">ID del token</param>
        /// <returns>True si es la sesión actual, false en caso contrario</returns>
        private bool IsCurrentSession(string tokenId)
        {
            string currentTokenId = User.FindFirst("jti")?.Value;
            return !string.IsNullOrEmpty(currentTokenId) && currentTokenId == tokenId;
        }
    }
}
