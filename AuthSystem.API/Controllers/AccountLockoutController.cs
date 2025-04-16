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
    [Route("api/account-lockout")]
    [Authorize(Roles = "Admin")]
    public class AccountLockoutController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountLockoutService _accountLockoutService;
        private readonly ILogger<AccountLockoutController> _logger;

        public AccountLockoutController(
            IUnitOfWork unitOfWork,
            IAccountLockoutService accountLockoutService,
            ILogger<AccountLockoutController> logger)
        {
            _unitOfWork = unitOfWork;
            _accountLockoutService = accountLockoutService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el estado de bloqueo de una cuenta de usuario
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

        /// <summary>
        /// Desbloquea una cuenta de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("unlock/{username}")]
        public async Task<IActionResult> UnlockAccount(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"Usuario '{username}' no encontrado");
                }

                await _accountLockoutService.UnlockAccountAsync(user);

                _logger.LogInformation("Usuario {Username} desbloqueado por {AdminUsername}", 
                    username, User.Identity.Name);

                return Ok(new { message = $"La cuenta del usuario '{username}' ha sido desbloqueada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear la cuenta del usuario {Username}", username);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
