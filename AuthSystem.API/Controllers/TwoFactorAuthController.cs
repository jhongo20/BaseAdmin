using System;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/two-factor")]
    public class TwoFactorAuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<TwoFactorAuthController> _logger;

        public TwoFactorAuthController(
            IUnitOfWork unitOfWork,
            ITwoFactorAuthService twoFactorAuthService,
            IJwtService jwtService,
            ILogger<TwoFactorAuthController> logger)
        {
            _unitOfWork = unitOfWork;
            _twoFactorAuthService = twoFactorAuthService;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Genera la información de configuración para la autenticación de dos factores
        /// </summary>
        /// <returns>Información de configuración</returns>
        [HttpPost("setup")]
        [Authorize]
        public async Task<ActionResult<TwoFactorSetupResponse>> Setup()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("uid")?.Value);
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                
                if (user == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                if (user.TwoFactorEnabled)
                {
                    return BadRequest("La autenticación de dos factores ya está habilitada para este usuario");
                }

                var setupInfo = await _twoFactorAuthService.GenerateSetupInfoAsync(user);
                return Ok(setupInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar información de configuración de 2FA");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Habilita la autenticación de dos factores para el usuario actual
        /// </summary>
        /// <param name="code">Código de verificación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("enable")]
        [Authorize]
        public async Task<IActionResult> Enable([FromBody] string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest("El código de verificación es requerido");
                }

                var userId = Guid.Parse(User.FindFirst("uid")?.Value);
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                
                if (user == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                if (user.TwoFactorEnabled)
                {
                    return BadRequest("La autenticación de dos factores ya está habilitada para este usuario");
                }

                bool result = await _twoFactorAuthService.EnableTwoFactorAsync(user, code);
                
                if (result)
                {
                    return Ok(new { 
                        message = "Autenticación de dos factores habilitada correctamente",
                        recoveryCode = user.TwoFactorRecoveryCode
                    });
                }
                else
                {
                    return BadRequest("Código de verificación inválido");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al habilitar 2FA");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Deshabilita la autenticación de dos factores para el usuario actual
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("disable")]
        [Authorize]
        public async Task<IActionResult> Disable()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst("uid")?.Value);
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                
                if (user == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                if (!user.TwoFactorEnabled)
                {
                    return BadRequest("La autenticación de dos factores no está habilitada para este usuario");
                }

                bool result = await _twoFactorAuthService.DisableTwoFactorAsync(user);
                
                if (result)
                {
                    return Ok(new { message = "Autenticación de dos factores deshabilitada correctamente" });
                }
                else
                {
                    return StatusCode(500, "Error al deshabilitar la autenticación de dos factores");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deshabilitar 2FA");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Verifica un código de autenticación de dos factores durante el inicio de sesión
        /// </summary>
        /// <param name="request">Solicitud de verificación</param>
        /// <returns>Respuesta de autenticación completa</returns>
        [HttpPost("verify")]
        public async Task<ActionResult<AuthResponse>> Verify([FromBody] TwoFactorVerifyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || 
                    string.IsNullOrEmpty(request.Code) || 
                    string.IsNullOrEmpty(request.SessionToken))
                {
                    return BadRequest("Todos los campos son requeridos");
                }

                // Validar el token de sesión
                bool isSessionValid = await _twoFactorAuthService.ValidateSessionTokenAsync(
                    request.Username, request.SessionToken);
                
                if (!isSessionValid)
                {
                    return Unauthorized("Sesión inválida o expirada");
                }

                // Obtener el usuario
                var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    return Unauthorized("Usuario no encontrado");
                }

                // Verificar el código
                bool isCodeValid = await _twoFactorAuthService.VerifyCodeAsync(user, request.Code);
                if (!isCodeValid)
                {
                    return Unauthorized("Código de verificación inválido");
                }

                // Obtener roles y permisos del usuario
                var userRoles = await _unitOfWork.UserRoles.GetByUserAsync(user.Id);
                var roles = userRoles.Select(ur => ur.Role.Name).ToList();

                var permissions = new List<string>();
                foreach (var userRole in userRoles)
                {
                    var rolePermissions = await _unitOfWork.Permissions.GetByRoleAsync(userRole.RoleId);
                    permissions.AddRange(rolePermissions.Select(p => p.Name));
                }

                // Eliminar duplicados en permisos
                permissions = permissions.Distinct().ToList();

                // Generar tokens
                string token = await _jwtService.GenerateTokenAsync(
                    user.Id,
                    user.Username,
                    user.Email,
                    roles,
                    permissions);

                string refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id);

                // Construir respuesta
                var response = new AuthResponse
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(60), // Obtener de la configuración
                    IsLdapUser = user.UserType == Domain.Common.Enums.UserType.Internal,
                    Roles = roles.ToArray(),
                    Permissions = permissions.ToArray()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la verificación de 2FA para el usuario {Username}", request.Username);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
