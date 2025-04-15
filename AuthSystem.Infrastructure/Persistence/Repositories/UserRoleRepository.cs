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
    /// Implementación del repositorio de relaciones usuario-rol
    /// </summary>
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UserRoleRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene relaciones usuario-rol por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol</returns>
        public async Task<IReadOnlyList<UserRole>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones usuario-rol activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol activas</returns>
        public async Task<IReadOnlyList<UserRole>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive && ur.Role.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones usuario-rol por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol</returns>
        public async Task<IReadOnlyList<UserRole>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.RoleId == roleId && ur.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una relación usuario-rol específica
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-rol encontrada o null</returns>
        public async Task<UserRole> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive, cancellationToken);
        }

        /// <summary>
        /// Asigna múltiples roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">Lista de IDs de roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol creadas</returns>
        public async Task<IReadOnlyList<UserRole>> AssignRolesToUserAsync(
            Guid userId, 
            IEnumerable<Guid> roleIds, 
            CancellationToken cancellationToken = default)
        {
            // Verificar que el usuario existe
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException($"No existe un usuario con ID {userId}", nameof(userId));
            }

            var createdUserRoles = new List<UserRole>();
            
            // Obtener las relaciones existentes para no duplicar
            var existingUserRoles = await _dbSet
                .Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId))
                .ToListAsync(cancellationToken);
            
            var existingRoleIds = existingUserRoles.Select(ur => ur.RoleId).ToHashSet();

            // Crear nuevas relaciones para los roles que no existen
            foreach (var roleId in roleIds)
            {
                if (!existingRoleIds.Contains(roleId))
                {
                    // Verificar que el rol existe
                    var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
                    if (role == null)
                    {
                        continue; // Ignorar roles que no existen
                    }

                    var userRole = new UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };

                    await _dbSet.AddAsync(userRole, cancellationToken);
                    createdUserRoles.Add(userRole);
                }
                else
                {
                    // Reactivar relaciones existentes que estén inactivas
                    var existingUserRole = existingUserRoles
                        .FirstOrDefault(ur => ur.RoleId == roleId && !ur.IsActive);
                    
                    if (existingUserRole != null)
                    {
                        existingUserRole.IsActive = true;
                        existingUserRole.LastModifiedBy = Guid.Empty.ToString();
                        existingUserRole.LastModifiedAt = DateTime.UtcNow;
                        createdUserRoles.Add(existingUserRole);
                    }
                }
            }

            return createdUserRoles;
        }

        /// <summary>
        /// Elimina todos los roles de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task RemoveAllRolesFromUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userRoles = await _dbSet
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var userRole in userRoles)
            {
                userRole.IsActive = false;
                userRole.LastModifiedBy = Guid.Empty.ToString();
                userRole.LastModifiedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Elimina roles específicos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">Lista de IDs de roles a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de relaciones eliminadas</returns>
        public async Task<int> RemoveRolesFromUserAsync(
            Guid userId, 
            IEnumerable<Guid> roleIds, 
            CancellationToken cancellationToken = default)
        {
            var userRoles = await _dbSet
                .Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId) && ur.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var userRole in userRoles)
            {
                userRole.IsActive = false;
                userRole.LastModifiedBy = Guid.Empty.ToString();
                userRole.LastModifiedAt = DateTime.UtcNow;
            }

            return userRoles.Count;
        }

        /// <summary>
        /// Obtiene una relación usuario-rol por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID de la relación usuario-rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-rol encontrada o null</returns>
        public override async Task<UserRole> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.Id == id && ur.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las relaciones usuario-rol activas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol activas</returns>
        public override async Task<IReadOnlyList<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <param name="assignedBy">ID del usuario que asigna el rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la asignación fue exitosa</returns>
        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid assignedBy, CancellationToken cancellationToken = default)
        {
            // Verificar si ya existe la relación
            var existingUserRole = await _dbSet
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

            if (existingUserRole != null && !existingUserRole.IsActive)
            {
                existingUserRole.IsActive = true;
                existingUserRole.LastModifiedBy = assignedBy.ToString();
                existingUserRole.LastModifiedAt = DateTime.UtcNow;
                return true;
            }

            // Crear nueva relación
            var newUserRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                CreatedBy = assignedBy.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _dbSet.AddAsync(newUserRole, cancellationToken);
            return true;
        }

        /// <summary>
        /// Remueve un rol de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <param name="removedBy">ID del usuario que remueve el rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la remoción fue exitosa</returns>
        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId, Guid removedBy, CancellationToken cancellationToken = default)
        {
            var userRole = await _dbSet
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive, cancellationToken);

            if (userRole == null)
            {
                return false;
            }

            userRole.IsActive = false;
            userRole.LastModifiedBy = removedBy.ToString();
            userRole.LastModifiedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Verifica si un usuario tiene un rol específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el rol</returns>
        public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive, cancellationToken);
        }

        /// <summary>
        /// Verifica si un usuario tiene un rol específico por nombre
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleName">Nombre del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el rol</returns>
        public async Task<bool> UserHasRoleByNameAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("El nombre del rol no puede ser nulo o vacío", nameof(roleName));
            }

            return await _dbSet
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName && ur.IsActive && ur.Role.IsActive, cancellationToken);
        }
    }
}
