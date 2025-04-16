using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers.v2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUnitOfWork unitOfWork, ILogger<UsersController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene todos los usuarios (Versión 2)
        /// </summary>
        /// <returns>Lista de usuarios con información mejorada</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Implementar paginación en la versión 2
                var allUsers = await _unitOfWork.Users.GetAllAsync();
                var totalUsers = allUsers.Count;
                var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
                
                // Validar parámetros de paginación
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;
                
                // Aplicar paginación manualmente
                var users = allUsers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                var response = new List<object>();
                foreach (var user in users)
                {
                    response.Add(new
                    {
                        Id = user.Id,
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLogin = user.LastLoginAt,
                        Status = user.Status.ToString()
                    });
                }
                
                return Ok(new
                {
                    Users = response,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = totalUsers,
                        TotalPages = totalPages,
                        HasPrevious = page > 1,
                        HasNext = page < totalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener usuarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID (Versión 2)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario con información detallada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado", statusCode = 404 });
                }
                
                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLoginAt,
                    Status = user.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener usuario", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Obtiene el perfil del usuario actual (Versión 2)
        /// </summary>
        /// <returns>Perfil del usuario actual</returns>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                // Obtener el ID del usuario actual
                if (!Guid.TryParse(User.FindFirst("sub")?.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado correctamente" });
                }
                
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    LastLogin = user.LastLoginAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener perfil de usuario", error = ex.Message });
            }
        }
    }
}
