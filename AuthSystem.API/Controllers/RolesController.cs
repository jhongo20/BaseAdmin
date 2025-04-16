using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IUnitOfWork unitOfWork, ILogger<RolesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                return StatusCode(500, "Error interno del servidor al obtener roles");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetById(Guid id)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound($"Rol con ID {id} no encontrado");
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol con ID {RoleId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el rol");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Role>> GetByName(string name)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByNameAsync(name);
                if (role == null)
                {
                    return NotFound($"Rol con nombre {name} no encontrado");
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol con nombre {Name}", name);
                return StatusCode(500, "Error interno del servidor al obtener el rol");
            }
        }

        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<Role>>> GetByOrganization(Guid organizationId)
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetByOrganizationAsync(organizationId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles de la organización con ID {OrganizationId}", organizationId);
                return StatusCode(500, "Error interno del servidor al obtener roles por organización");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Role>>> GetByUser(Guid userId)
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetByUserAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles del usuario con ID {UserId}", userId);
                return StatusCode(500, "Error interno del servidor al obtener roles por usuario");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Role>>> Search(
            [FromQuery] string searchTerm = null,
            [FromQuery] Guid? organizationId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool includePermissions = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Como no existe SearchAsync, utilizamos GetAllAsync y filtramos manualmente
                IReadOnlyList<Role> roles;
                
                if (includePermissions)
                {
                    roles = await _unitOfWork.Roles.GetWithPermissionsAsync();
                }
                else
                {
                    roles = await _unitOfWork.Roles.GetAllAsync();
                }
                
                var filteredRoles = roles.AsQueryable();
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredRoles = filteredRoles.Where(r => 
                        r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                        r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }
                
                if (organizationId.HasValue)
                {
                    filteredRoles = filteredRoles.Where(r => r.OrganizationId == organizationId.Value);
                }
                
                if (isActive.HasValue)
                {
                    filteredRoles = filteredRoles.Where(r => r.IsActive == isActive.Value);
                }
                
                // Aplicar paginación
                var result = filteredRoles
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar roles con los criterios especificados");
                return StatusCode(500, "Error interno del servidor al buscar roles");
            }
        }

        [HttpGet("{roleId}/permissions")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions(Guid roleId)
        {
            try
            {
                var permissions = await _unitOfWork.Permissions.GetByRoleAsync(roleId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del rol con ID {RoleId}", roleId);
                return StatusCode(500, "Error interno del servidor al obtener permisos del rol");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole(Role role)
        {
            try
            {
                var newRole = await _unitOfWork.Roles.AddAsync(role);
                await _unitOfWork.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = newRole.Id }, newRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                return StatusCode(500, "Error interno del servidor al crear rol");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(Guid id, Role role)
        {
            try
            {
                var existingRole = await _unitOfWork.Roles.GetByIdAsync(id);
                if (existingRole == null)
                {
                    return NotFound($"Rol con ID {id} no encontrado");
                }

                existingRole.Name = role.Name;
                existingRole.Description = role.Description;
                existingRole.IsActive = role.IsActive;

                await _unitOfWork.Roles.UpdateAsync(existingRole);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol con ID {RoleId}", id);
                return StatusCode(500, "Error interno del servidor al actualizar rol");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(Guid id)
        {
            try
            {
                var existingRole = await _unitOfWork.Roles.GetByIdAsync(id);
                if (existingRole == null)
                {
                    return NotFound($"Rol con ID {id} no encontrado");
                }

                await _unitOfWork.Roles.DeleteAsync(existingRole);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol con ID {RoleId}", id);
                return StatusCode(500, "Error interno del servidor al eliminar rol");
            }
        }
    }
}
