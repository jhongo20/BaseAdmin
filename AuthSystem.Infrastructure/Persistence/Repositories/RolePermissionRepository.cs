using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de relaciones rol-permiso
    /// </summary>
    public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public RolePermissionRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene una relación rol-permiso por rol y permiso
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación rol-permiso encontrada o null</returns>
        public async Task<RolePermission> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones rol-permiso por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso del rol</returns>
        public async Task<IReadOnlyList<RolePermission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .OrderBy(rp => rp.Permission.Module.Name)
                .ThenBy(rp => rp.Permission.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones rol-permiso por permiso
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso del permiso</returns>
        public async Task<IReadOnlyList<RolePermission>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.PermissionId == permissionId && rp.IsActive)
                .OrderBy(rp => rp.Role.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones rol-permiso activas por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso activas</returns>
        public async Task<IReadOnlyList<RolePermission>> GetActiveByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await GetByRoleAsync(roleId, cancellationToken);
        }

        /// <summary>
        /// Asigna un permiso a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="assignedBy">ID del usuario que asigna el permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la asignación fue exitosa</returns>
        public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, Guid assignedBy, CancellationToken cancellationToken = default)
        {
            // Verificar si ya existe la relación
            var existingRolePermission = await _dbSet
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if (existingRolePermission != null)
            {
                // Si existe pero está inactiva, reactivarla
                if (!existingRolePermission.IsActive)
                {
                    existingRolePermission.IsActive = true;
                    existingRolePermission.LastModifiedAt = DateTime.UtcNow;
                    return true;
                }
                // Si ya está activa, no hacer nada
                return false;
            }

            // Crear nueva relación
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedBy = assignedBy.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _dbSet.AddAsync(rolePermission, cancellationToken);
            return true;
        }

        /// <summary>
        /// Asigna múltiples permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">Lista de IDs de permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso creadas</returns>
        public async Task<IReadOnlyList<RolePermission>> AssignPermissionsToRoleAsync(
            Guid roleId, 
            IEnumerable<Guid> permissionIds, 
            CancellationToken cancellationToken = default)
        {
            // Verificar que los permisos existen
            var permissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (permissions.Count == 0)
            {
                return new List<RolePermission>();
            }

            // Obtener las relaciones existentes
            var existingRolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissions.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken);

            var existingPermissionIds = existingRolePermissions
                .Where(rp => rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToHashSet();

            var result = new List<RolePermission>();

            // Reactivar relaciones inactivas
            var inactiveRolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId) && !rp.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var rp in inactiveRolePermissions)
            {
                rp.IsActive = true;
                rp.LastModifiedAt = DateTime.UtcNow;
                result.Add(rp);
            }

            // Crear nuevas relaciones para los permisos que no existen
            var newPermissionIds = permissions.Except(existingRolePermissions.Select(rp => rp.PermissionId));
            foreach (var permissionId in newPermissionIds)
            {
                var newRolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _dbSet.AddAsync(newRolePermission, cancellationToken);
                result.Add(newRolePermission);
            }

            return result;
        }

        /// <summary>
        /// Asigna múltiples permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">Lista de IDs de permisos</param>
        /// <param name="assignedBy">ID del usuario que asigna los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de permisos asignados</returns>
        public async Task<int> AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid assignedBy, CancellationToken cancellationToken = default)
        {
            // Verificar que los permisos existen
            var permissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (permissions.Count == 0)
            {
                return 0;
            }

            // Obtener las relaciones existentes
            var existingRolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissions.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken);

            var existingPermissionIds = existingRolePermissions
                .Where(rp => rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToHashSet();

            int assignedCount = 0;

            // Reactivar relaciones inactivas
            var inactiveRolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId) && !rp.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var rp in inactiveRolePermissions)
            {
                rp.IsActive = true;
                rp.LastModifiedAt = DateTime.UtcNow;
                assignedCount++;
            }

            // Crear nuevas relaciones para los permisos que no existen
            var newPermissionIds = permissions.Except(existingRolePermissions.Select(rp => rp.PermissionId));
            foreach (var permissionId in newPermissionIds)
            {
                var newRolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedBy = assignedBy.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _dbSet.AddAsync(newRolePermission, cancellationToken);
                assignedCount++;
            }

            return assignedCount;
        }

        /// <summary>
        /// Remueve un permiso de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="removedBy">ID del usuario que remueve el permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la remoción fue exitosa</returns>
        public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, Guid removedBy, CancellationToken cancellationToken = default)
        {
            var rolePermission = await _dbSet
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.IsActive, cancellationToken);

            if (rolePermission == null)
            {
                return false;
            }

            rolePermission.IsActive = false;
            rolePermission.LastModifiedBy = removedBy.ToString();
            rolePermission.LastModifiedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Remueve múltiples permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">Lista de IDs de permisos</param>
        /// <param name="removedBy">ID del usuario que remueve los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de permisos removidos</returns>
        public async Task<int> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid removedBy, CancellationToken cancellationToken = default)
        {
            var rolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId) && rp.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var rolePermission in rolePermissions)
            {
                rolePermission.IsActive = false;
                rolePermission.LastModifiedBy = removedBy.ToString();
                rolePermission.LastModifiedAt = DateTime.UtcNow;
            }

            return rolePermissions.Count;
        }

        /// <summary>
        /// Elimina todos los permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var rolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var rolePermission in rolePermissions)
            {
                rolePermission.IsActive = false;
                rolePermission.LastModifiedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Obtiene todos los permisos de un usuario a través de sus roles
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso para el usuario</returns>
        public async Task<IReadOnlyList<RolePermission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Obtener los roles del usuario
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            if (userRoleIds.Count == 0)
            {
                return new List<RolePermission>();
            }

            // Obtener los permisos asociados a esos roles
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
                .Where(rp => userRoleIds.Contains(rp.RoleId) && rp.IsActive)
                .OrderBy(rp => rp.Permission.Module.Name)
                .ThenBy(rp => rp.Permission.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca relaciones rol-permiso con criterios avanzados
        /// </summary>
        /// <param name="roleId">ID del rol (opcional)</param>
        /// <param name="permissionId">ID del permiso (opcional)</param>
        /// <param name="moduleId">ID del módulo (opcional)</param>
        /// <param name="isActive">Indica si está activa (opcional)</param>
        /// <param name="includeRole">Indica si se debe incluir el rol</param>
        /// <param name="includePermission">Indica si se debe incluir el permiso</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de relaciones rol-permiso que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<RolePermission> Items, int TotalCount)> SearchAsync(
            Guid? roleId = null,
            Guid? permissionId = null,
            Guid? moduleId = null,
            bool? isActive = null,
            bool includeRole = false,
            bool includePermission = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<RolePermission> query = _dbSet;

            // Incluir relaciones según se solicite
            if (includeRole)
            {
                query = query.Include(rp => rp.Role);
            }

            if (includePermission)
            {
                query = query.Include(rp => rp.Permission)
                    .ThenInclude(p => p.Module);
            }

            // Aplicar filtros
            if (roleId.HasValue)
            {
                query = query.Where(rp => rp.RoleId == roleId.Value);
            }

            if (permissionId.HasValue)
            {
                query = query.Where(rp => rp.PermissionId == permissionId.Value);
            }

            if (moduleId.HasValue)
            {
                query = query.Where(rp => rp.Permission.ModuleId == moduleId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(rp => rp.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(rp => rp.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(rp => rp.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene una relación rol-permiso por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID de la relación rol-permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación rol-permiso encontrada o null</returns>
        public override async Task<RolePermission> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
                .FirstOrDefaultAsync(rp => rp.Id == id && rp.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las relaciones rol-permiso activas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso activas</returns>
        public override async Task<IReadOnlyList<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
                .Where(rp => rp.IsActive)
                .OrderBy(rp => rp.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
