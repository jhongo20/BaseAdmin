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
    public class PermissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IUnitOfWork unitOfWork, ILogger<PermissionsController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAll()
        {
            try
            {
                var permissions = await _unitOfWork.Permissions.GetAllAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los permisos");
                return StatusCode(500, "Error interno del servidor al obtener permisos");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetById(Guid id)
        {
            try
            {
                var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
                if (permission == null)
                {
                    return NotFound($"Permiso con ID {id} no encontrado");
                }
                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso con ID {PermissionId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el permiso");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Permission>> GetByName(string name)
        {
            try
            {
                var permission = await _unitOfWork.Permissions.GetByNameAsync(name);
                if (permission == null)
                {
                    return NotFound($"Permiso con nombre {name} no encontrado");
                }
                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso con nombre {Name}", name);
                return StatusCode(500, "Error interno del servidor al obtener el permiso");
            }
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<Permission>> GetByCode(string code)
        {
            try
            {
                // Utilizamos GetByNameAsync ya que no existe GetByCodeAsync
                var permission = await _unitOfWork.Permissions.GetByNameAsync(code);
                if (permission == null)
                {
                    return NotFound($"Permiso con código {code} no encontrado");
                }
                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso con código {Code}", code);
                return StatusCode(500, "Error interno del servidor al obtener el permiso");
            }
        }

        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetByModule(Guid moduleId)
        {
            try
            {
                var permissions = await _unitOfWork.Permissions.GetByModuleAsync(moduleId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del módulo con ID {ModuleId}", moduleId);
                return StatusCode(500, "Error interno del servidor al obtener permisos por módulo");
            }
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetByRole(Guid roleId)
        {
            try
            {
                var permissions = await _unitOfWork.Permissions.GetByRoleAsync(roleId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del rol con ID {RoleId}", roleId);
                return StatusCode(500, "Error interno del servidor al obtener permisos por rol");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetByUser(Guid userId)
        {
            try
            {
                var permissions = await _unitOfWork.Permissions.GetByUserAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del usuario con ID {UserId}", userId);
                return StatusCode(500, "Error interno del servidor al obtener permisos por usuario");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Permission>>> Search(
            [FromQuery] string searchTerm = null,
            [FromQuery] Guid? moduleId = null,
            [FromQuery] string type = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Como no existe SearchAsync, utilizamos GetAllAsync y filtramos manualmente
                var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
                var filteredPermissions = allPermissions.AsQueryable();
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredPermissions = filteredPermissions.Where(p => 
                        p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                        p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }
                
                if (moduleId.HasValue)
                {
                    filteredPermissions = filteredPermissions.Where(p => p.ModuleId == moduleId.Value);
                }
                
                if (!string.IsNullOrEmpty(type))
                {
                    // Intentar convertir el string a PermissionType
                    if (Enum.TryParse<PermissionType>(type, true, out var permissionType))
                    {
                        filteredPermissions = filteredPermissions.Where(p => p.Type == permissionType);
                    }
                }
                
                if (isActive.HasValue)
                {
                    filteredPermissions = filteredPermissions.Where(p => p.IsActive == isActive.Value);
                }
                
                // Aplicar paginación
                var result = filteredPermissions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar permisos con los criterios especificados");
                return StatusCode(500, "Error interno del servidor al buscar permisos");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Permission>> Create(Permission permission)
        {
            try
            {
                var newPermission = await _unitOfWork.Permissions.AddAsync(permission);
                await _unitOfWork.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = newPermission.Id }, newPermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso");
                return StatusCode(500, "Error interno del servidor al crear permiso");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, Permission permission)
        {
            try
            {
                var existingPermission = await _unitOfWork.Permissions.GetByIdAsync(id);
                if (existingPermission == null)
                {
                    return NotFound($"Permiso con ID {id} no encontrado");
                }
                
                existingPermission.Name = permission.Name;
                existingPermission.Description = permission.Description;
                existingPermission.Type = permission.Type;
                existingPermission.IsActive = permission.IsActive;
                existingPermission.ModuleId = permission.ModuleId;
                
                await _unitOfWork.Permissions.UpdateAsync(existingPermission);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso con ID {PermissionId}", id);
                return StatusCode(500, "Error interno del servidor al actualizar permiso");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
                if (permission == null)
                {
                    return NotFound($"Permiso con ID {id} no encontrado");
                }
                
                await _unitOfWork.Permissions.DeleteAsync(permission);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso con ID {PermissionId}", id);
                return StatusCode(500, "Error interno del servidor al eliminar permiso");
            }
        }
    }
}
