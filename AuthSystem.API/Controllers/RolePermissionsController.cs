using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RolePermissionsController> _logger;

        public RolePermissionsController(IUnitOfWork unitOfWork, ILogger<RolePermissionsController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="request">Permisos a asignar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("roles/{roleId}/permissions")]
        public async Task<ActionResult> AssignPermissionsToRole(Guid roleId, RolePermissionAssignmentRequest request)
        {
            try
            {
                // Verificar si el rol existe
                var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound($"Rol con ID {roleId} no encontrado");
                }

                // Verificar que todos los permisos existan
                foreach (var permissionId in request.PermissionIds)
                {
                    var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
                    if (permission == null)
                    {
                        return BadRequest($"Permiso con ID {permissionId} no encontrado");
                    }
                }

                // Obtener los permisos actuales del rol
                var currentPermissions = await _unitOfWork.Permissions.GetByRoleAsync(roleId);
                var currentPermissionIds = currentPermissions.Select(p => p.Id).ToHashSet();

                // Determinar permisos a añadir (los que están en la solicitud pero no en los actuales)
                var permissionsToAdd = request.PermissionIds.Where(id => !currentPermissionIds.Contains(id)).ToList();

                // Añadir nuevos permisos al rol
                foreach (var permissionId in permissionsToAdd)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    };
                    await _unitOfWork.RolePermissions.AddAsync(rolePermission);
                }

                await _unitOfWork.SaveChangesAsync();
                return Ok(new { Message = "Permisos asignados correctamente al rol" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar permisos al rol con ID {RoleId}", roleId);
                return StatusCode(500, "Error interno del servidor al asignar permisos al rol");
            }
        }

        /// <summary>
        /// Elimina un permiso específico de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        public async Task<ActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            try
            {
                // Verificar si el rol existe
                var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound($"Rol con ID {roleId} no encontrado");
                }

                // Verificar si el permiso existe
                var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    return NotFound($"Permiso con ID {permissionId} no encontrado");
                }

                // Buscar la relación rol-permiso
                var rolePermission = await _unitOfWork.RolePermissions.GetByRoleAndPermissionAsync(roleId, permissionId);
                if (rolePermission == null)
                {
                    return NotFound($"El rol no tiene asignado el permiso especificado");
                }

                // Eliminar la relación
                await _unitOfWork.RolePermissions.DeleteAsync(rolePermission);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { Message = "Permiso eliminado correctamente del rol" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso {PermissionId} del rol {RoleId}", permissionId, roleId);
                return StatusCode(500, "Error interno del servidor al eliminar permiso del rol");
            }
        }

        /// <summary>
        /// Actualiza todos los permisos de un rol (reemplaza los existentes)
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="request">Nuevos permisos</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("roles/{roleId}/permissions")]
        public async Task<ActionResult> UpdateRolePermissions(Guid roleId, RolePermissionAssignmentRequest request)
        {
            try
            {
                // Verificar si el rol existe
                var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound($"Rol con ID {roleId} no encontrado");
                }

                // Verificar que todos los permisos existan
                foreach (var permissionId in request.PermissionIds)
                {
                    var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
                    if (permission == null)
                    {
                        return BadRequest($"Permiso con ID {permissionId} no encontrado");
                    }
                }

                // Obtener todas las relaciones rol-permiso actuales
                var currentRolePermissions = await _unitOfWork.RolePermissions.GetByRoleAsync(roleId);

                // Eliminar todas las relaciones actuales
                foreach (var rolePermission in currentRolePermissions)
                {
                    await _unitOfWork.RolePermissions.DeleteAsync(rolePermission);
                }

                // Crear nuevas relaciones para cada permiso en la solicitud
                foreach (var permissionId in request.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    };
                    await _unitOfWork.RolePermissions.AddAsync(rolePermission);
                }

                await _unitOfWork.SaveChangesAsync();
                return Ok(new { Message = "Permisos del rol actualizados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permisos del rol con ID {RoleId}", roleId);
                return StatusCode(500, "Error interno del servidor al actualizar permisos del rol");
            }
        }
    }
}
