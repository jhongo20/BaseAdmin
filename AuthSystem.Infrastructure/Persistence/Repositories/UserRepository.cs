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
    /// Implementación del repositorio de usuarios
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("El nombre de usuario no puede ser nulo o vacío", nameof(username));
            }

            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("El correo electrónico no puede ser nulo o vacío", nameof(email));
            }

            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su ID externo (LDAP, SSO, etc.)
        /// </summary>
        /// <param name="externalId">ID externo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        public async Task<User> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(externalId))
            {
                throw new ArgumentException("El ID externo no puede ser nulo o vacío", nameof(externalId));
            }

            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.LdapDN == externalId && u.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario por su DN de LDAP
        /// </summary>
        /// <param name="ldapDN">DN de LDAP</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        public async Task<User> GetByLdapDNAsync(string ldapDN, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ldapDN))
            {
                throw new ArgumentException("El DN de LDAP no puede ser nulo o vacío", nameof(ldapDN));
            }

            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.LdapDN == ldapDN && u.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene usuarios por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios de la organización</returns>
        public async Task<IReadOnlyList<User>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .Where(u => u.UserBranches.Any(ub => ub.Branch.OrganizationId == organizationId) && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene usuarios por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios de la sucursal</returns>
        public async Task<IReadOnlyList<User>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .Where(u => u.UserBranches.Any(ub => ub.BranchId == branchId && ub.IsActive) && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios con el rol</returns>
        public async Task<IReadOnlyList<User>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId && ur.IsActive) && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un usuario con todos sus roles y permisos
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario con roles y permisos</returns>
        public async Task<User> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                                .ThenInclude(p => p.Module)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        }

        /// <summary>
        /// Busca usuarios con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, correo, etc.)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="roleId">ID del rol (opcional)</param>
        /// <param name="userType">Tipo de usuario (opcional)</param>
        /// <param name="isActive">Indica si está activo (opcional)</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de usuarios que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            Guid? organizationId = null,
            Guid? roleId = null,
            UserType? userType = null,
            bool? isActive = null,
            bool includeRoles = false,
            bool includeBranches = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<User> query = _dbSet;

            // Incluir relaciones según se solicite
            if (includeRoles)
            {
                query = query.Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role);
            }

            if (includeBranches)
            {
                query = query.Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch);
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(searchTerm) || 
                    u.Email.ToLower().Contains(searchTerm) || 
                    u.FullName.ToLower().Contains(searchTerm));
            }

            if (organizationId.HasValue)
            {
                // Filtrar por organización a través de las sucursales del usuario
                query = query.Where(u => u.UserBranches.Any(ub => 
                    ub.Branch.OrganizationId == organizationId.Value && 
                    ub.IsActive && 
                    ub.Branch.IsActive));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId.Value && ur.IsActive));
            }

            if (userType.HasValue)
            {
                query = query.Where(u => u.UserType == userType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(u => u.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene un usuario por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        public override async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los usuarios activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios activos</returns>
        public override async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);
        }
    }
}
