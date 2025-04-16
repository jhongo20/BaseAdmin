using System;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/lockout-test")]
    public class AccountLockoutTestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountLockoutService _accountLockoutService;
        private readonly ILogger<AccountLockoutTestController> _logger;

        public AccountLockoutTestController(
            IUnitOfWork unitOfWork,
            IAccountLockoutService accountLockoutService,
            ILogger<AccountLockoutTestController> logger)
        {
            _unitOfWork = unitOfWork;
            _accountLockoutService = accountLockoutService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de prueba para simular un intento fallido de inicio de sesión
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Información sobre el intento fallido</returns>
        [HttpPost("failed-attempt/{username}")]
        public async Task<IActionResult> SimulateFailedLoginAttempt(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"Usuario '{username}' no encontrado");
                }

                bool isLocked = await _accountLockoutService.RecordFailedLoginAttemptAsync(user);
                
                var response = new
                {
                    Username = user.Username,
                    AccessFailedCount = user.AccessFailedCount,
                    IsLocked = isLocked,
                    LockoutEnd = user.LockoutEnd,
                    RemainingAttempts = Math.Max(0, 5 - user.AccessFailedCount) // Asumiendo configuración predeterminada
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular intento fallido para el usuario {Username}", username);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Endpoint de prueba para simular un inicio de sesión exitoso
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Información sobre el inicio de sesión exitoso</returns>
        [HttpPost("successful-login/{username}")]
        public async Task<IActionResult> SimulateSuccessfulLogin(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"Usuario '{username}' no encontrado");
                }

                await _accountLockoutService.RecordSuccessfulLoginAttemptAsync(user);
                
                var response = new
                {
                    Username = user.Username,
                    AccessFailedCount = user.AccessFailedCount,
                    IsLocked = _accountLockoutService.IsAccountLocked(user),
                    LastLoginAt = user.LastLoginAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al simular inicio de sesión exitoso para el usuario {Username}", username);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar el estado de bloqueo de una cuenta
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Estado de bloqueo de la cuenta</returns>
        [HttpGet("status/{username}")]
        public async Task<ActionResult<AccountLockoutStatusResponse>> GetLockoutStatus(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"Usuario '{username}' no encontrado");
                }

                var isLocked = _accountLockoutService.IsAccountLocked(user);
                var remainingMinutes = 0.0;

                if (isLocked && user.LockoutEnd.HasValue)
                {
                    remainingMinutes = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                }

                return Ok(new AccountLockoutStatusResponse
                {
                    Username = user.Username,
                    IsLocked = isLocked,
                    AccessFailedCount = user.AccessFailedCount,
                    LockoutEnd = user.LockoutEnd,
                    RemainingMinutes = remainingMinutes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de bloqueo para el usuario {Username}", username);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
