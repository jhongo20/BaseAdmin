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
    /// Implementación del repositorio de módulos
    /// </summary>
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public ModuleRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un módulo por su nombre
        /// </summary>
        /// <param name="name">Nombre del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        public async Task<Module> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre del módulo no puede ser nulo o vacío", nameof(name));
            }

            return await _dbSet
                .Include(m => m.Permissions)
                .Include(m => m.Parent)
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Name == name && m.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un módulo por su ruta
        /// </summary>
        /// <param name="route">Ruta del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        public async Task<Module> GetByRouteAsync(string route, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(route))
            {
                throw new ArgumentException("La ruta del módulo no puede ser nula o vacía", nameof(route));
            }

            return await _dbSet
                .Include(m => m.Permissions)
                .Include(m => m.Parent)
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Route == route && m.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene módulos principales (sin padre)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos principales</returns>
        public async Task<IReadOnlyList<Module>> GetMainModulesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Permissions)
                .Include(m => m.Children)
                .Where(m => m.ParentId == null && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene submódulos de un módulo padre
        /// </summary>
        /// <param name="parentId">ID del módulo padre</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de submódulos</returns>
        public async Task<IReadOnlyList<Module>> GetSubmodulesAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Permissions)
                .Where(m => m.ParentId == parentId && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene los módulos hijos de un módulo
        /// </summary>
        /// <param name="parentModuleId">ID del módulo padre</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos hijos</returns>
        public async Task<IReadOnlyList<Module>> GetChildModulesAsync(Guid parentModuleId, CancellationToken cancellationToken = default)
        {
            return await GetSubmodulesAsync(parentModuleId, cancellationToken);
        }

        /// <summary>
        /// Obtiene el árbol completo de módulos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Árbol de módulos</returns>
        public async Task<IReadOnlyList<Module>> GetModuleTreeAsync(CancellationToken cancellationToken = default)
        {
            // Obtener todos los módulos activos con sus relaciones
            var allModules = await _dbSet
                .Include(m => m.Permissions)
                .Include(m => m.Children)
                .Where(m => m.IsActive)
                .ToListAsync(cancellationToken);

            // Filtrar solo los módulos principales (sin padre)
            var mainModules = allModules.Where(m => m.ParentId == null).ToList();

            return mainModules;
        }

        /// <summary>
        /// Obtiene módulos por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos de la organización</returns>
        public async Task<IReadOnlyList<Module>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            // Obtener los roles de la organización
            var roleIds = await _context.Roles
                .Where(r => r.OrganizationId == organizationId && r.IsActive)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (roleIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los permisos asociados a esos roles
            var permissionIds = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            if (permissionIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los módulos asociados a esos permisos
            var moduleIds = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive)
                .Select(p => p.ModuleId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await _dbSet
                .Include(m => m.Permissions)
                .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene módulos por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos del usuario</returns>
        public async Task<IReadOnlyList<Module>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Obtener los roles del usuario
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            if (roleIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los permisos asociados a esos roles
            var permissionIds = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            if (permissionIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los módulos asociados a esos permisos
            var moduleIds = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive)
                .Select(p => p.ModuleId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await _dbSet
                .Include(m => m.Permissions)
                .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene módulos por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos del rol</returns>
        public async Task<IReadOnlyList<Module>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            // Obtener los permisos del rol
            var permissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            if (permissionIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los módulos asociados a esos permisos
            var moduleIds = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive)
                .Select(p => p.ModuleId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await _dbSet
                .Include(m => m.Permissions)
                .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene módulos con sus permisos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos con sus permisos</returns>
        public async Task<IReadOnlyList<Module>> GetWithPermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Permissions)
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un módulo con sus permisos
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo con sus permisos</returns>
        public async Task<Module> GetWithPermissionsAsync(Guid moduleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Permissions)
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene módulos accesibles para un usuario según sus permisos
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos accesibles</returns>
        public async Task<IReadOnlyList<Module>> GetAccessibleByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Obtener los roles del usuario
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            if (roleIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los permisos asociados a esos roles
            var permissionIds = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            if (permissionIds.Count == 0)
            {
                return new List<Module>();
            }

            // Obtener los módulos asociados a esos permisos
            var moduleIds = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id) && p.IsActive)
                .Select(p => p.ModuleId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Obtener los módulos y sus padres
            var modules = await _dbSet
                .Include(m => m.Permissions)
                .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
                .ToListAsync(cancellationToken);

            // Obtener los IDs de los módulos padres
            var parentIds = modules
                .Where(m => m.ParentId.HasValue)
                .Select(m => m.ParentId.Value)
                .Distinct()
                .ToList();

            // Obtener los módulos padres que no están en la lista original
            var parentModules = await _dbSet
                .Where(m => parentIds.Contains(m.Id) && !moduleIds.Contains(m.Id) && m.IsActive)
                .ToListAsync(cancellationToken);

            // Combinar las listas y ordenar
            return modules
                .Union(parentModules)
                .OrderBy(m => m.DisplayOrder)
                .ToList();
        }

        /// <summary>
        /// Busca módulos con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción, ruta)</param>
        /// <param name="isActive">Indica si está activo (opcional)</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de módulos que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<Module> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            bool? isActive = null,
            bool includePermissions = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Module> query = _dbSet;

            // Incluir relaciones según se solicite
            if (includePermissions)
            {
                query = query.Include(m => m.Permissions);
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(searchTerm) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchTerm)) ||
                    (m.Route != null && m.Route.ToLower().Contains(searchTerm)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(m => m.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene un módulo por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        public override async Task<Module> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Permissions)
                .Include(m => m.Parent)
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los módulos activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos activos</returns>
        public override async Task<IReadOnlyList<Module>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Permissions)
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
