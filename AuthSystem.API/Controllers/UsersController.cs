using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUnitOfWork unitOfWork, ILogger<UsersController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode(500, "Error interno del servidor al obtener usuarios");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el usuario");
            }
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetByUsername(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"Usuario con nombre de usuario {username} no encontrado");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con nombre de usuario {Username}", username);
                return StatusCode(500, "Error interno del servidor al obtener el usuario");
            }
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetByEmail(string email)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"Usuario con correo electrónico {email} no encontrado");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con correo electrónico {Email}", email);
                return StatusCode(500, "Error interno del servidor al obtener el usuario");
            }
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetByRole(Guid roleId)
        {
            try
            {
                var users = await _unitOfWork.Users.GetByRoleAsync(roleId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios con rol ID {RoleId}", roleId);
                return StatusCode(500, "Error interno del servidor al obtener usuarios por rol");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<User>>> Search(
            [FromQuery] string searchTerm = null,
            [FromQuery] Guid? organizationId = null,
            [FromQuery] Guid? roleId = null,
            [FromQuery] string userType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool includeRoles = false,
            [FromQuery] bool includeBranches = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Como no existe SearchAsync, implementamos la búsqueda manualmente
                IReadOnlyList<User> users;
                
                if (includeRoles && includeBranches)
                {
                    // Obtener todos los usuarios con sus relaciones
                    users = await _unitOfWork.Users.GetAllAsync();
                    // Cargar manualmente las relaciones (esto es una simplificación, en un caso real
                    // sería mejor implementar un método específico en el repositorio)
                }
                else
                {
                    users = await _unitOfWork.Users.GetAllAsync();
                }
                
                var filteredUsers = users.AsQueryable();
                
                // Aplicar filtros
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredUsers = filteredUsers.Where(u => 
                        u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                        u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                        u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }
                
                if (roleId.HasValue)
                {
                    filteredUsers = filteredUsers.Where(u => 
                        u.UserRoles.Any(ur => ur.RoleId == roleId.Value && ur.IsActive));
                }
                
                if (organizationId.HasValue)
                {
                    filteredUsers = filteredUsers.Where(u => 
                        u.UserBranches.Any(ub => ub.Branch.OrganizationId == organizationId.Value && ub.IsActive));
                }
                
                if (!string.IsNullOrEmpty(userType))
                {
                    // Intentar convertir el string a UserType
                    if (Enum.TryParse<UserType>(userType, true, out var userTypeEnum))
                    {
                        filteredUsers = filteredUsers.Where(u => u.UserType == userTypeEnum);
                    }
                }
                
                if (isActive.HasValue)
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive == isActive.Value);
                }
                
                // Aplicar paginación
                var result = filteredUsers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuarios con los criterios especificados");
                return StatusCode(500, "Error interno del servidor al buscar usuarios");
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            try
            {
                var newUser = await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, "Error interno del servidor al crear usuario");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, User user)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FullName = user.FullName;
                existingUser.UserType = user.UserType;
                existingUser.IsActive = user.IsActive;

                await _unitOfWork.Users.UpdateAsync(existingUser);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al actualizar usuario");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"Usuario con ID {id} no encontrado");
                }

                await _unitOfWork.Users.DeleteAsync(existingUser);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al eliminar usuario");
            }
        }
    }
}
