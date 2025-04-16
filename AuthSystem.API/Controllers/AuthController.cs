using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BC = BCrypt.Net.BCrypt;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly ILdapService _ldapService;
        private readonly IAccountLockoutService _accountLockoutService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            ILdapService ldapService,
            IAccountLockoutService accountLockoutService,
            ILogger<AuthController> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _ldapService = ldapService;
            _accountLockoutService = accountLockoutService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Username and password are required");
                }

                User user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    return Unauthorized("Invalid username or password");
                }

                if (!user.IsActive)
                {
                    return Unauthorized("User account is inactive");
                }

                // Verificar si la cuenta está bloqueada
                if (_accountLockoutService.IsAccountLocked(user))
                {
                    var remainingMinutes = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                    return Unauthorized($"Account is locked. Please try again after {remainingMinutes} minute(s)");
                }

                bool isAuthenticated = false;

                // Autenticación basada en el tipo de usuario
                if (request.IsLdapUser || user.UserType == UserType.Internal)
                {
                    // Autenticación LDAP para usuarios internos
                    Guid organizationId = Guid.Empty;
                    var userBranches = await _unitOfWork.UserBranches.GetByUserAsync(user.Id);
                    if (userBranches.Any())
                    {
                        var branch = userBranches.First();
                        organizationId = branch.Branch.OrganizationId;
                    }

                    isAuthenticated = await _ldapService.AuthenticateAsync(request.Username, request.Password, organizationId);
                }
                else
                {
                    // Autenticación local para usuarios externos
                    isAuthenticated = BC.Verify(request.Password, user.PasswordHash);
                }

                if (!isAuthenticated)
                {
                    // Registrar intento fallido y posiblemente bloquear la cuenta
                    await _accountLockoutService.RecordFailedLoginAttemptAsync(user);
                    return Unauthorized("Invalid username or password");
                }

                // Registrar inicio de sesión exitoso y resetear contador de intentos fallidos
                await _accountLockoutService.RecordSuccessfulLoginAttemptAsync(user);

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

                // Registrar el inicio de sesión exitoso
                await LogSuccessfulLogin(user.Id, "Web", GetClientIpAddress());

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
                    IsLdapUser = user.UserType == UserType.Internal,
                    Roles = roles.ToArray(),
                    Permissions = permissions.ToArray()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return StatusCode(500, "An error occurred during login");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest("Refresh token is required");
                }

                var (isValid, userId) = await _jwtService.ValidateRefreshTokenAsync(request.RefreshToken);
                if (!isValid)
                {
                    return Unauthorized("Invalid refresh token");
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized("User not found or inactive");
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

                // Generar nuevo token
                string token = await _jwtService.GenerateTokenAsync(
                    user.Id,
                    user.Username,
                    user.Email,
                    roles,
                    permissions);

                // Generar nuevo refresh token
                string refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id);

                // Revocar el refresh token anterior
                await _jwtService.RevokeTokenAsync(request.RefreshToken, "Token refreshed", CancellationToken.None);

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
                    IsLdapUser = user.UserType == UserType.Internal,
                    Roles = roles.ToArray(),
                    Permissions = permissions.ToArray()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, "An error occurred while refreshing token");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID not found in token");
                }

                // Revocar todas las sesiones activas del usuario
                var userSessions = await _unitOfWork.UserSessions.GetActiveByUserAsync(Guid.Parse(userId));
                foreach (var session in userSessions)
                {
                    session.IsActive = false;
                    session.LastModifiedAt = DateTime.UtcNow;
                    await _unitOfWork.UserSessions.UpdateAsync(session);
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout");
            }
        }

        [Authorize]
        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken()
        {
            // Si llegamos aquí, el token es válido
            return Ok(new { isValid = true });
        }

        private async Task LogSuccessfulLogin(Guid userId, string deviceInfo, string ipAddress)
        {
            try
            {
                // Registrar el inicio de sesión exitoso
                var userSession = new UserSession
                {
                    UserId = userId,
                    LastActivity = DateTime.UtcNow,
                    UserAgent = deviceInfo,
                    IpAddress = ipAddress,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserSessions.AddAsync(userSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging successful login for user {UserId}", userId);
            }
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        }
    }
}
