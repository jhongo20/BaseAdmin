using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de permisos
    /// </summary>
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public PermissionRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        /// <param name="name">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        public async Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre del permiso no puede ser nulo o vacío", nameof(name));
            }

            return await _dbSet
                .Include(p => p.Module)
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Name == name && p.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un permiso por su código
        /// </summary>
        /// <param name="code">Código del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        public async Task<Permission> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("El código del permiso no puede ser nulo o vacío", nameof(code));
            }

            return await _dbSet
                .Include(p => p.Module)
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Name == code && p.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un permiso por su nombre y módulo
        /// </summary>
        /// <param name="name">Nombre del permiso</param>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        public async Task<Permission> GetByNameAndModuleAsync(string name, Guid moduleId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre del permiso no puede ser nulo o vacío", nameof(name));
            }

            return await _dbSet
                .Include(p => p.Module)
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Name == name && p.ModuleId == moduleId && p.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene permisos por módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del módulo</returns>
        public async Task<IReadOnlyList<Permission>> GetByModuleAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Module)
                .Where(p => p.ModuleId == moduleId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene permisos por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del rol</returns>
        public async Task<IReadOnlyList<Permission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Module)
                .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId && rp.IsActive) && p.IsActive)
                .OrderBy(p => p.Module.Name)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene permisos por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del usuario</returns>
        public async Task<IReadOnlyList<Permission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Obtener los roles del usuario
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            if (userRoles.Count == 0)
            {
                return new List<Permission>();
            }

            // Obtener los permisos asociados a esos roles
            return await _dbSet
                .Include(p => p.Module)
                .Where(p => p.RolePermissions.Any(rp => userRoles.Contains(rp.RoleId) && rp.IsActive) && p.IsActive)
                .OrderBy(p => p.Module.Name)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso</returns>
        public async Task<bool> UserHasPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            // Obtener los roles del usuario
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            if (userRoles.Count == 0)
            {
                return false;
            }

            // Verificar si alguno de los roles tiene el permiso
            return await _context.RolePermissions
                .AnyAsync(rp => 
                    userRoles.Contains(rp.RoleId) && 
                    rp.PermissionId == permissionId && 
                    rp.IsActive, 
                    cancellationToken);
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico por nombre
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso</returns>
        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                throw new ArgumentException("El nombre del permiso no puede ser nulo o vacío", nameof(permissionName));
            }

            // Obtener el permiso por su nombre
            var permission = await _dbSet
                .FirstOrDefaultAsync(p => p.Name == permissionName && p.IsActive, cancellationToken);

            if (permission == null)
            {
                return false;
            }

            return await UserHasPermissionAsync(userId, permission.Id, cancellationToken);
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico por código
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionCode">Código del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso</returns>
        public async Task<bool> UserHasPermissionByCodeAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permissionCode))
            {
                throw new ArgumentException("El código del permiso no puede ser nulo o vacío", nameof(permissionCode));
            }

            // Obtener el permiso por su código (usando el nombre como código)
            var permission = await _dbSet
                .FirstOrDefaultAsync(p => p.Name == permissionCode && p.IsActive, cancellationToken);

            if (permission == null)
            {
                return false;
            }

            return await UserHasPermissionAsync(userId, permission.Id, cancellationToken);
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico en un módulo
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso en el módulo</returns>
        public async Task<bool> UserHasPermissionInModuleAsync(Guid userId, Guid moduleId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(permissionName))
            {
                throw new ArgumentException("El nombre del permiso no puede ser nulo o vacío", nameof(permissionName));
            }

            // Obtener el permiso por su nombre y módulo
            var permission = await _dbSet
                .FirstOrDefaultAsync(p => p.Name == permissionName && p.ModuleId == moduleId && p.IsActive, cancellationToken);

            if (permission == null)
            {
                return false;
            }

            return await UserHasPermissionAsync(userId, permission.Id, cancellationToken);
        }

        /// <summary>
        /// Obtiene permisos por tipo
        /// </summary>
        /// <param name="permissionType">Tipo de permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del tipo especificado</returns>
        public async Task<IReadOnlyList<Permission>> GetByTypeAsync(PermissionType permissionType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Module)
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .Where(p => p.Type == permissionType && p.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca permisos con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción)</param>
        /// <param name="moduleId">ID del módulo (opcional)</param>
        /// <param name="permissionType">Tipo de permiso (opcional)</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de permisos que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<Permission> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            Guid? moduleId = null,
            PermissionType? permissionType = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Permission> query = _dbSet
                .Include(p => p.Module)
                .Where(p => p.IsActive);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            if (moduleId.HasValue)
            {
                query = query.Where(p => p.ModuleId == moduleId.Value);
            }

            if (permissionType.HasValue)
            {
                query = query.Where(p => p.Type == permissionType.Value);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(p => p.Module.Name)
                .ThenBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene un permiso por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        public override async Task<Permission> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Module)
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los permisos activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos activos</returns>
        public override async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Module)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module.Name)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Crea permisos estándar para un módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="permissionTypes">Tipos de permisos a crear</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos creados</returns>
        public async Task<IReadOnlyList<Permission>> CreateStandardPermissionsForModuleAsync(
            Guid moduleId,
            IEnumerable<PermissionType> permissionTypes,
            CancellationToken cancellationToken = default)
        {
            var module = await _context.Modules.FindAsync(new object[] { moduleId }, cancellationToken);
            if (module == null)
            {
                throw new ArgumentException($"No existe un módulo con ID {moduleId}", nameof(moduleId));
            }

            var createdPermissions = new List<Permission>();
            foreach (var permissionType in permissionTypes)
            {
                var permissionName = $"{module.Name}.{permissionType}";
                var existingPermission = await GetByNameAndModuleAsync(permissionName, moduleId, cancellationToken);
                
                if (existingPermission == null)
                {
                    var permission = new Permission
                    {
                        Name = permissionName,
                        Description = $"Permiso para {permissionType.ToString().ToLower()} en el módulo {module.Name}",
                        Type = permissionType,
                        ModuleId = moduleId
                    };

                    await _dbSet.AddAsync(permission, cancellationToken);
                    createdPermissions.Add(permission);
                }
            }

            return createdPermissions;
        }
    }
}
