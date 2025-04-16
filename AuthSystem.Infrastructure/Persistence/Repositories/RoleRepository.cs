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
    /// Implementación del repositorio de roles
    /// </summary>
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre del rol no puede ser nulo o vacío", nameof(name));
            }

            return await _dbSet
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == name && r.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        public async Task<Role> GetByNameAsync(string name, Guid? organizationId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre del rol no puede ser nulo o vacío", nameof(name));
            }

            var query = _dbSet
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => r.Name == name && r.IsActive);

            if (organizationId.HasValue)
            {
                query = query.Where(r => r.OrganizationId == organizationId.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un rol por su nombre y organización
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        public async Task<Role> GetByNameAndOrganizationAsync(string name, Guid organizationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre del rol no puede ser nulo o vacío", nameof(name));
            }

            return await _dbSet
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == name && r.OrganizationId == organizationId && r.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene roles por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles de la organización</returns>
        public async Task<IReadOnlyList<Role>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => r.OrganizationId == organizationId && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene roles por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles del usuario</returns>
        public async Task<IReadOnlyList<Role>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => r.UserRoles.Any(ur => ur.UserId == userId && ur.IsActive) && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene roles por usuario y organización
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles del usuario en la organización</returns>
        public async Task<IReadOnlyList<Role>> GetByUserAndOrganizationAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => 
                    r.UserRoles.Any(ur => ur.UserId == userId && ur.IsActive) && 
                    r.OrganizationId == organizationId && 
                    r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los roles con sus permisos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles con sus permisos</returns>
        public async Task<IReadOnlyList<Role>> GetWithPermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                        .ThenInclude(p => p.Module)
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un rol con sus permisos
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol con sus permisos</returns>
        public async Task<Role> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                        .ThenInclude(p => p.Module)
                .FirstOrDefaultAsync(r => r.Id == roleId && r.IsActive, cancellationToken);
        }

        /// <summary>
        /// Busca roles con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="isActive">Indica si está activo (opcional)</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de roles que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<Role> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            Guid? organizationId = null,
            bool? isActive = null,
            bool includePermissions = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Role> query = _dbSet;

            // Incluir relaciones según se solicite
            if (includePermissions)
            {
                query = query.Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission);
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(searchTerm) ||
                    (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
            }

            if (organizationId.HasValue)
            {
                query = query.Where(r => r.OrganizationId == organizationId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(r => r.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene un rol por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        public override async Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.UserRoles)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los roles activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles activos</returns>
        public override async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene roles que tienen asignado un permiso específico
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles que tienen el permiso</returns>
        public async Task<IReadOnlyList<Role>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.RolePermissions)
                .Where(r => r.RolePermissions.Any(rp => rp.PermissionId == permissionId && rp.IsActive) && 
                            r.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
