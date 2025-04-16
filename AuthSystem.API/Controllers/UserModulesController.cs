using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserModulesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserModulesController> _logger;

        public UserModulesController(IUnitOfWork unitOfWork, ILogger<UserModulesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene los usuarios que tienen acceso a un módulo específico
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <returns>Lista de usuarios con acceso al módulo</returns>
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<IEnumerable<UserModuleResponse>>> GetUsersByModule(Guid moduleId)
        {
            try
            {
                // Verificar si el módulo existe
                var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);
                if (module == null)
                {
                    return NotFound($"Módulo con ID {moduleId} no encontrado");
                }

                // Obtener todos los permisos del módulo
                var modulePermissions = await _unitOfWork.Permissions.GetByModuleAsync(moduleId);
                
                // Obtener todos los roles que tienen estos permisos
                var roleIds = new HashSet<Guid>();
                foreach (var permission in modulePermissions)
                {
                    var rolesWithPermission = await _unitOfWork.Roles.GetByPermissionAsync(permission.Id);
                    foreach (var role in rolesWithPermission)
                    {
                        roleIds.Add(role.Id);
                    }
                }

                // Obtener todos los usuarios con estos roles
                var userModuleResponses = new List<UserModuleResponse>();
                var processedUserIds = new HashSet<Guid>();
                
                foreach (var roleId in roleIds)
                {
                    var usersWithRole = await _unitOfWork.Users.GetByRoleAsync(roleId);
                    var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                    
                    foreach (var user in usersWithRole)
                    {
                        // Evitar duplicados
                        if (processedUserIds.Contains(user.Id))
                        {
                            // Actualizar roles para usuarios ya procesados
                            var existingUser = userModuleResponses.First(u => u.Id == user.Id);
                            if (!existingUser.Roles.Any(r => r.Id == role.Id))
                            {
                                var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
                                existingUser.Roles.Add(new UserRoleInfo
                                {
                                    Id = role.Id,
                                    Name = role.Name,
                                    Description = role.Description,
                                    IsActive = role.IsActive,
                                    ValidFrom = userRole?.ValidFrom,
                                    ValidTo = userRole?.ValidTo
                                });
                            }
                            continue;
                        }

                        processedUserIds.Add(user.Id);
                        
                        var userResponse = new UserModuleResponse
                        {
                            Id = user.Id,
                            Username = user.Username,
                            FullName = user.FullName,
                            Email = user.Email,
                            Status = user.Status,
                            UserType = user.UserType,
                            LastLogin = user.LastLoginAt,
                            Roles = new List<UserRoleInfo>(),
                            DetailInfo = new UserDetailInfo
                            {
                                PhoneNumber = user.PhoneNumber,
                                CreatedAt = user.CreatedAt,
                                LastModifiedAt = user.LastModifiedAt,
                                FailedLoginAttempts = user.FailedLoginAttempts,
                                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow,
                                LockoutEnd = user.LockoutEnd,
                                TwoFactorEnabled = user.TwoFactorEnabled
                            }
                        };
                        
                        // Añadir el rol actual
                        var currentUserRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
                        userResponse.Roles.Add(new UserRoleInfo
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description,
                            IsActive = role.IsActive,
                            ValidFrom = currentUserRole?.ValidFrom,
                            ValidTo = currentUserRole?.ValidTo
                        });
                        
                        userModuleResponses.Add(userResponse);
                    }
                }

                return Ok(userModuleResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios del módulo con ID {ModuleId}", moduleId);
                return StatusCode(500, "Error interno del servidor al obtener usuarios por módulo");
            }
        }

        /// <summary>
        /// Obtiene todos los módulos con sus usuarios agrupados
        /// </summary>
        /// <returns>Lista de módulos con sus usuarios</returns>
        [HttpGet("grouped")]
        public async Task<ActionResult<IEnumerable<ModuleUsersGroupResponse>>> GetUsersGroupedByModule()
        {
            try
            {
                // Obtener todos los módulos
                var modules = await _unitOfWork.Modules.GetAllAsync();
                var result = new List<ModuleUsersGroupResponse>();
                
                foreach (var module in modules)
                {
                    // Obtener todos los permisos del módulo
                    var modulePermissions = await _unitOfWork.Permissions.GetByModuleAsync(module.Id);
                    
                    // Obtener todos los roles que tienen estos permisos
                    var roleIds = new HashSet<Guid>();
                    foreach (var permission in modulePermissions)
                    {
                        var rolesWithPermission = await _unitOfWork.Roles.GetByPermissionAsync(permission.Id);
                        foreach (var role in rolesWithPermission)
                        {
                            roleIds.Add(role.Id);
                        }
                    }
                    
                    // Crear la respuesta para este módulo
                    var moduleResponse = new ModuleUsersGroupResponse
                    {
                        Id = module.Id,
                        Name = module.Name,
                        Description = module.Description,
                        Route = module.Route,
                        Icon = module.Icon,
                        IsEnabled = module.IsEnabled,
                        Users = new List<ModuleUserInfo>()
                    };
                    
                    // Obtener usuarios para cada rol
                    var processedUserIds = new HashSet<Guid>();
                    
                    foreach (var roleId in roleIds)
                    {
                        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                        var usersWithRole = await _unitOfWork.Users.GetByRoleAsync(roleId);
                        
                        foreach (var user in usersWithRole)
                        {
                            if (processedUserIds.Contains(user.Id))
                            {
                                // Actualizar roles para usuarios ya procesados
                                var existingUser = moduleResponse.Users.First(u => u.Id == user.Id);
                                if (!existingUser.Roles.Contains(role.Name))
                                {
                                    existingUser.Roles.Add(role.Name);
                                }
                                continue;
                            }
                            
                            processedUserIds.Add(user.Id);
                            
                            moduleResponse.Users.Add(new ModuleUserInfo
                            {
                                Id = user.Id,
                                Username = user.Username,
                                FullName = user.FullName,
                                Email = user.Email,
                                Roles = new List<string> { role.Name },
                                Status = user.Status.ToString()
                            });
                        }
                    }
                    
                    moduleResponse.UserCount = moduleResponse.Users.Count;
                    result.Add(moduleResponse);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios agrupados por módulo");
                return StatusCode(500, "Error interno del servidor al obtener usuarios agrupados por módulo");
            }
        }

        /// <summary>
        /// Obtiene los permisos de un usuario para una ruta específica
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="route">Ruta a verificar</param>
        /// <returns>Permisos del usuario para la ruta especificada</returns>
        [HttpGet("user/{userId}/route-permissions")]
        public async Task<ActionResult<UserRoutePermissionsResponse>> GetUserRoutePermissions(Guid userId, [FromQuery] string route)
        {
            try
            {
                if (string.IsNullOrEmpty(route))
                {
                    return BadRequest("La ruta es obligatoria");
                }
                
                // Verificar si el usuario existe
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound($"Usuario con ID {userId} no encontrado");
                }
                
                // Normalizar la ruta (eliminar / inicial si existe)
                route = route.TrimStart('/');
                
                // Encontrar el módulo que coincide con la ruta
                var modules = await _unitOfWork.Modules.GetAllAsync();
                var matchingModule = modules.FirstOrDefault(m => 
                    !string.IsNullOrEmpty(m.Route) && 
                    route.StartsWith(m.Route.TrimStart('/'), StringComparison.OrdinalIgnoreCase));
                
                if (matchingModule == null)
                {
                    return NotFound($"No se encontró ningún módulo que coincida con la ruta: {route}");
                }
                
                // Obtener todos los permisos del usuario
                var userPermissions = await _unitOfWork.Permissions.GetByUserAsync(userId);
                
                // Filtrar permisos por módulo
                var modulePermissions = userPermissions.Where(p => p.ModuleId == matchingModule.Id).ToList();
                
                // Crear la respuesta
                var response = new UserRoutePermissionsResponse
                {
                    UserId = userId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Route = route,
                    Module = new ModuleInfo
                    {
                        Id = matchingModule.Id,
                        Name = matchingModule.Name,
                        BaseRoute = matchingModule.Route
                    },
                    Permissions = new List<RoutePermissionInfo>(),
                    HasAccess = modulePermissions.Any()
                };
                
                // Obtener los roles del usuario
                var userRoles = await _unitOfWork.Roles.GetByUserAsync(userId);
                
                // Añadir información detallada de permisos
                foreach (var permission in modulePermissions)
                {
                    // Encontrar a través de qué rol se obtiene este permiso
                    foreach (var role in userRoles)
                    {
                        var rolePermissions = await _unitOfWork.Permissions.GetByRoleAsync(role.Id);
                        if (rolePermissions.Any(rp => rp.Id == permission.Id))
                        {
                            response.Permissions.Add(new RoutePermissionInfo
                            {
                                Id = permission.Id,
                                Name = permission.Name,
                                Description = permission.Description,
                                Type = permission.Type,
                                SourceRole = role.Name,
                                SourceRoleId = role.Id
                            });
                            break;
                        }
                    }
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos de usuario para la ruta");
                return StatusCode(500, "Error interno del servidor al obtener permisos de usuario para la ruta");
            }
        }
    }
}
