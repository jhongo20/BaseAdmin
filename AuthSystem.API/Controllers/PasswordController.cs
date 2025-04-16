using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordController> _logger;
        private readonly IConfiguration _configuration;

        public PasswordController(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<PasswordController> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Solicita un restablecimiento de contraseña
        /// </summary>
        /// <param name="request">Datos de la solicitud</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("reset-request")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
        {
            try
            {
                // Buscar al usuario por correo electrónico
                var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    // No revelar si el correo existe o no por seguridad
                    return Ok(new { Message = "Si el correo está registrado, recibirás instrucciones para restablecer tu contraseña" });
                }

                // Generar token de restablecimiento
                string resetToken = await _jwtService.GeneratePasswordResetTokenAsync(user.Id);
                
                // Guardar el token en la base de datos
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Construir URL de restablecimiento
                string resetUrl = $"{Request.Scheme}://{Request.Host}/reset-password";

                // Enviar correo electrónico
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    user.Username,
                    user.FullName,
                    resetToken,
                    resetUrl,
                    CancellationToken.None);

                // Registrar en auditoría
                await LogAuditAsync("PasswordResetRequest", user.Id.ToString(), user.Id);

                return Ok(new { Message = "Si el correo está registrado, recibirás instrucciones para restablecer tu contraseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar restablecimiento de contraseña para {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Confirma el restablecimiento de contraseña
        /// </summary>
        /// <param name="confirmation">Datos de confirmación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("reset-confirm")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmation confirmation)
        {
            try
            {
                // Validar el token
                var userId = await _jwtService.ValidatePasswordResetTokenAsync(confirmation.Token);
                if (userId == null)
                {
                    return BadRequest(new { Message = "Token inválido o expirado" });
                }

                // Buscar al usuario
                var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new { Message = "Usuario no encontrado" });
                }

                // Verificar que el token coincida con el almacenado y no haya expirado
                if (user.PasswordResetToken != confirmation.Token || 
                    user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { Message = "Token inválido o expirado" });
                }

                // Actualizar la contraseña
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(confirmation.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.LastPasswordChangeAt = DateTime.UtcNow;
                
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Registrar en auditoría
                await LogAuditAsync("PasswordResetConfirm", user.Id.ToString(), user.Id);

                // Enviar notificación de cambio de contraseña
                await _emailService.SendSecurityAlertEmailAsync(
                    user.Email,
                    user.Username,
                    user.FullName,
                    "Cambio de contraseña",
                    "Se ha restablecido la contraseña de tu cuenta.",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    HttpContext.Request.Headers["User-Agent"].ToString(),
                    CancellationToken.None);

                return Ok(new { Message = "Contraseña restablecida correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar restablecimiento de contraseña");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado
        /// </summary>
        /// <param name="request">Datos para el cambio de contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("change")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Obtener el ID del usuario autenticado
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado" });
                }

                // Buscar al usuario
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "Usuario no encontrado" });
                }

                // Verificar la contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { Message = "Contraseña actual incorrecta" });
                }

                // Actualizar la contraseña
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.LastPasswordChangeAt = DateTime.UtcNow;
                
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Registrar en auditoría
                await LogAuditAsync("PasswordChange", user.Id.ToString(), user.Id);

                // Enviar notificación de cambio de contraseña
                await _emailService.SendSecurityAlertEmailAsync(
                    user.Email,
                    user.Username,
                    user.FullName,
                    "Cambio de contraseña",
                    "Se ha cambiado la contraseña de tu cuenta.",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    HttpContext.Request.Headers["User-Agent"].ToString(),
                    CancellationToken.None);

                return Ok(new { Message = "Contraseña cambiada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error al procesar la solicitud" });
            }
        }

        private async Task LogAuditAsync(string action, string entityId, Guid? userId)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityName = "User",
                    EntityId = entityId,
                    UserId = userId,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    Timestamp = DateTime.UtcNow,
                    ActionType = DetermineActionType(action),
                    Endpoint = HttpContext.Request.Path,
                    QueryString = HttpContext.Request.QueryString.ToString(),
                    Severity = "Information"
                };

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar auditoría para {Action}", action);
            }
        }

        private AuditActionType DetermineActionType(string action)
        {
            switch (action)
            {
                case "PasswordResetRequest":
                    return AuditActionType.Update;
                case "PasswordResetConfirm":
                    return AuditActionType.Update;
                case "PasswordChange":
                    return AuditActionType.Update;
                default:
                    return AuditActionType.Read;
            }
        }
    }
}
