using System;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Activa una cuenta de usuario
        /// </summary>
        /// <param name="request">Datos de activación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("activate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateAccount([FromBody] AccountActivationRequest request)
        {
            try
            {
                // Validar el token
                var userId = await _jwtService.ValidateAccountActivationTokenAsync(request.Token);
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
                if (user.ActivationToken != request.Token || 
                    user.ActivationTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { Message = "Token inválido o expirado" });
                }

                // Activar la cuenta
                user.IsActive = true;
                user.ActivationToken = null;
                user.ActivationTokenExpiry = null;
                user.ActivatedAt = DateTime.UtcNow;
                
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Registrar en auditoría
                await LogAuditAsync("AccountActivation", user.Id.ToString(), user.Id);

                // Enviar correo de bienvenida
                await _emailService.SendEmailAsync(
                    user.Email,
                    "¡Bienvenido a nuestro sistema!",
                    $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #4a6da7; color: white; padding: 10px; text-align: center; }}
                            .content {{ padding: 20px; border: 1px solid #ddd; }}
                            .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>¡Bienvenido!</h2>
                            </div>
                            <div class='content'>
                                <p>Hola {user.Username},</p>
                                <p>Tu cuenta ha sido activada correctamente.</p>
                                <p>Ya puedes iniciar sesión en nuestro sistema y comenzar a utilizarlo.</p>
                                <p>Saludos,<br>El equipo de soporte</p>
                            </div>
                            <div class='footer'>
                                <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                            </div>
                        </div>
                    </body>
                    </html>");

                return Ok(new { Message = "Cuenta activada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar cuenta");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Reenvía el correo de activación
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("resend-activation")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendActivation([FromBody] string email)
        {
            try
            {
                // Buscar al usuario por correo electrónico
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null)
                {
                    // No revelar si el correo existe o no por seguridad
                    return Ok(new { Message = "Si el correo está registrado y la cuenta no está activada, recibirás un nuevo correo de activación" });
                }

                // Verificar si la cuenta ya está activada
                if (user.IsActive)
                {
                    return BadRequest(new { Message = "La cuenta ya está activada" });
                }

                // Generar nuevo token de activación
                string activationToken = await _jwtService.GenerateAccountActivationTokenAsync(user.Id);
                
                // Guardar el token en la base de datos
                user.ActivationToken = activationToken;
                user.ActivationTokenExpiry = DateTime.UtcNow.AddHours(24);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Construir URL de activación
                string activationUrl = $"{Request.Scheme}://{Request.Host}/activate-account";

                // Enviar correo electrónico
                await _emailService.SendAccountActivationEmailAsync(
                    user.Email,
                    user.Username,
                    user.FullName,
                    activationToken,
                    activationUrl,
                    CancellationToken.None);

                // Registrar en auditoría
                await LogAuditAsync("ResendActivation", user.Id.ToString(), user.Id);

                return Ok(new { Message = "Si el correo está registrado y la cuenta no está activada, recibirás un nuevo correo de activación" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar correo de activación");
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
                case "AccountActivation":
                    return AuditActionType.Update;
                case "ResendActivation":
                    return AuditActionType.Update;
                default:
                    return AuditActionType.Read;
            }
        }
    }
}
