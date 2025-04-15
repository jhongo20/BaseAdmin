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
    /// Implementación del repositorio de relaciones usuario-sucursal
    /// </summary>
    public class UserBranchRepository : Repository<UserBranch>, IUserBranchRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UserBranchRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene una relación usuario-sucursal por usuario y sucursal
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal encontrada o null</returns>
        public async Task<UserBranch> GetByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.User)
                .Include(ub => ub.Branch)
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId && ub.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones usuario-sucursal por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal del usuario</returns>
        public async Task<IReadOnlyList<UserBranch>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.Branch)
                    .ThenInclude(b => b.Organization)
                .Where(ub => ub.UserId == userId && ub.IsActive)
                .OrderBy(ub => ub.Branch.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones usuario-sucursal por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal de la sucursal</returns>
        public async Task<IReadOnlyList<UserBranch>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.User)
                .Where(ub => ub.BranchId == branchId && ub.IsActive)
                .OrderBy(ub => ub.User.FullName)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene relaciones usuario-sucursal activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal activas</returns>
        public async Task<IReadOnlyList<UserBranch>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.User)
                .Include(ub => ub.Branch)
                    .ThenInclude(b => b.Organization)
                .Where(ub => ub.UserId == userId && ub.IsActive && ub.Branch.IsActive)
                .OrderBy(ub => ub.Branch.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene la sucursal principal de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal principal o null</returns>
        public async Task<UserBranch> GetPrimaryBranchForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.Branch)
                    .ThenInclude(b => b.Organization)
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.IsPrimary && ub.IsActive, cancellationToken);
        }

        /// <summary>
        /// Asigna una sucursal a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="isPrimary">Indica si es la sucursal principal</param>
        /// <param name="assignedBy">ID del usuario que asigna la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la asignación fue exitosa</returns>
        public async Task<bool> AssignBranchToUserAsync(Guid userId, Guid branchId, bool isPrimary, Guid assignedBy, CancellationToken cancellationToken = default)
        {
            // Verificar si ya existe la relación
            var existingUserBranch = await _dbSet
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId, cancellationToken);

            if (existingUserBranch != null)
            {
                // Si existe pero está inactiva, reactivarla
                if (!existingUserBranch.IsActive)
                {
                    existingUserBranch.IsActive = true;
                    existingUserBranch.LastModifiedBy = assignedBy.ToString();
                    existingUserBranch.LastModifiedAt = DateTime.UtcNow;
                    
                    // Actualizar si es primaria
                    if (isPrimary && !existingUserBranch.IsPrimary)
                    {
                        await RemovePrimaryFlagFromOtherBranchesAsync(userId, branchId, assignedBy, cancellationToken);
                        existingUserBranch.IsPrimary = true;
                    }
                    
                    return true;
                }
                
                // Si ya está activa, actualizar solo si cambia el estado de primaria
                if (isPrimary && !existingUserBranch.IsPrimary)
                {
                    await RemovePrimaryFlagFromOtherBranchesAsync(userId, branchId, assignedBy, cancellationToken);
                    existingUserBranch.IsPrimary = true;
                    existingUserBranch.LastModifiedBy = assignedBy.ToString();
                    existingUserBranch.LastModifiedAt = DateTime.UtcNow;
                    return true;
                }
                
                return false;
            }

            // Si se está marcando como primaria, quitar la marca de las demás
            if (isPrimary)
            {
                await RemovePrimaryFlagFromOtherBranchesAsync(userId, branchId, assignedBy, cancellationToken);
            }
            // Si no hay ninguna sucursal asignada, marcar esta como primaria independientemente del parámetro
            else if (!await _dbSet.AnyAsync(ub => ub.UserId == userId && ub.IsActive, cancellationToken))
            {
                isPrimary = true;
            }

            // Crear nueva relación
            var newUserBranch = new UserBranch
            {
                UserId = userId,
                BranchId = branchId,
                IsPrimary = isPrimary,
                CreatedBy = assignedBy.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _dbSet.AddAsync(newUserBranch, cancellationToken);
            return true;
        }

        /// <summary>
        /// Asigna múltiples sucursales a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchIds">Lista de IDs de sucursales</param>
        /// <param name="primaryBranchId">ID de la sucursal principal (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal creadas</returns>
        public async Task<IReadOnlyList<UserBranch>> AssignBranchesToUserAsync(
            Guid userId, 
            IEnumerable<Guid> branchIds,
            Guid? primaryBranchId = null,
            CancellationToken cancellationToken = default)
        {
            // Verificar que el usuario existe
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                throw new ArgumentException($"No existe un usuario con ID {userId}", nameof(userId));
            }

            var createdUserBranches = new List<UserBranch>();
            
            // Obtener las relaciones existentes para no duplicar
            var existingUserBranches = await _dbSet
                .Where(ub => ub.UserId == userId && branchIds.Contains(ub.BranchId))
                .ToListAsync(cancellationToken);
            
            var existingBranchIds = existingUserBranches.Select(ub => ub.BranchId).ToHashSet();

            // Si hay una sucursal principal, verificar que está en la lista de sucursales
            if (primaryBranchId.HasValue && !branchIds.Contains(primaryBranchId.Value))
            {
                throw new ArgumentException("La sucursal principal debe estar incluida en la lista de sucursales a asignar", nameof(primaryBranchId));
            }

            // Desmarcar cualquier sucursal principal existente
            if (primaryBranchId.HasValue)
            {
                var existingPrimaryBranches = await _dbSet
                    .Where(ub => ub.UserId == userId && ub.IsPrimary && ub.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var existingPrimaryBranch in existingPrimaryBranches)
                {
                    existingPrimaryBranch.IsPrimary = false;
                    existingPrimaryBranch.LastModifiedAt = DateTime.UtcNow;
                }
            }

            // Crear nuevas relaciones para las sucursales que no existen
            foreach (var branchId in branchIds)
            {
                bool isPrimary = primaryBranchId.HasValue && branchId == primaryBranchId.Value;

                if (!existingBranchIds.Contains(branchId))
                {
                    // Verificar que la sucursal existe
                    var branch = await _context.Branches.FindAsync(new object[] { branchId }, cancellationToken);
                    if (branch == null)
                    {
                        continue; // Ignorar sucursales que no existen
                    }

                    var userBranch = new UserBranch
                    {
                        UserId = userId,
                        BranchId = branchId,
                        IsPrimary = isPrimary,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _dbSet.AddAsync(userBranch, cancellationToken);
                    createdUserBranches.Add(userBranch);
                }
                else
                {
                    // Reactivar relaciones existentes que estén inactivas
                    var existingUserBranch = existingUserBranches
                        .FirstOrDefault(ub => ub.BranchId == branchId);
                    
                    if (existingUserBranch != null)
                    {
                        existingUserBranch.IsActive = true;
                        existingUserBranch.IsPrimary = isPrimary;
                        existingUserBranch.LastModifiedAt = DateTime.UtcNow;
                        createdUserBranches.Add(existingUserBranch);
                    }
                }
            }

            return createdUserBranches;
        }

        /// <summary>
        /// Remueve una sucursal de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="removedBy">ID del usuario que remueve la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la remoción fue exitosa</returns>
        public async Task<bool> RemoveBranchFromUserAsync(Guid userId, Guid branchId, Guid removedBy, CancellationToken cancellationToken = default)
        {
            var userBranch = await _dbSet
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId && ub.IsActive, cancellationToken);

            if (userBranch == null)
            {
                return false;
            }

            var wasPrimary = userBranch.IsPrimary;
            
            userBranch.IsActive = false;
            userBranch.LastModifiedBy = removedBy.ToString();
            userBranch.LastModifiedAt = DateTime.UtcNow;
            
            // Si era la sucursal principal, asignar otra como principal
            if (wasPrimary)
            {
                var anotherBranch = await _dbSet
                    .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId != branchId && ub.IsActive, cancellationToken);
                
                if (anotherBranch != null)
                {
                    anotherBranch.IsPrimary = true;
                    anotherBranch.LastModifiedBy = removedBy.ToString();
                    anotherBranch.LastModifiedAt = DateTime.UtcNow;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Elimina todas las asignaciones de sucursales para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RemoveAllBranchesFromUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userBranches = await _dbSet
                .Where(ub => ub.UserId == userId && ub.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var userBranch in userBranches)
            {
                userBranch.IsActive = false;
                userBranch.IsPrimary = false;
                userBranch.LastModifiedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Establece una sucursal como la principal para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal actualizada</returns>
        public async Task<UserBranch> SetPrimaryBranchForUserAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            // Verificar que existe la relación usuario-sucursal
            var userBranch = await GetByUserAndBranchAsync(userId, branchId, cancellationToken);
            if (userBranch == null)
            {
                // La relación no existe, intentar crearla
                var branch = await _context.Branches.FindAsync(new object[] { branchId }, cancellationToken);
                if (branch == null)
                {
                    throw new ArgumentException($"No existe una sucursal con ID {branchId}", nameof(branchId));
                }

                var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
                if (user == null)
                {
                    throw new ArgumentException($"No existe un usuario con ID {userId}", nameof(userId));
                }

                userBranch = new UserBranch
                {
                    UserId = userId,
                    BranchId = branchId,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _dbSet.AddAsync(userBranch, cancellationToken);
            }
            else
            {
                userBranch.IsPrimary = true;
                userBranch.LastModifiedAt = DateTime.UtcNow;
            }

            // Desmarcar cualquier otra sucursal principal
            var otherPrimaryBranches = await _dbSet
                .Where(ub => ub.UserId == userId && ub.BranchId != branchId && ub.IsPrimary && ub.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var otherPrimaryBranch in otherPrimaryBranches)
            {
                otherPrimaryBranch.IsPrimary = false;
                otherPrimaryBranch.LastModifiedAt = DateTime.UtcNow;
            }

            return userBranch;
        }

        /// <summary>
        /// Establece una sucursal como la principal para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="modifiedBy">ID del usuario que realiza el cambio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el cambio fue exitoso</returns>
        public async Task<bool> SetPrimaryBranchForUserAsync(Guid userId, Guid branchId, Guid modifiedBy, CancellationToken cancellationToken = default)
        {
            // Verificar que la relación existe y está activa
            var userBranch = await _dbSet
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId && ub.IsActive, cancellationToken);

            if (userBranch == null)
            {
                return false;
            }

            // Si ya es la principal, no hacer nada
            if (userBranch.IsPrimary)
            {
                return true;
            }

            // Quitar la marca de primaria de las demás sucursales
            await RemovePrimaryFlagFromOtherBranchesAsync(userId, branchId, modifiedBy, cancellationToken);

            // Marcar esta como primaria
            userBranch.IsPrimary = true;
            userBranch.LastModifiedBy = modifiedBy.ToString();
            userBranch.LastModifiedAt = DateTime.UtcNow;

            return true;
        }

        /// <summary>
        /// Verifica si un usuario tiene asignada una sucursal
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene la sucursal asignada</returns>
        public async Task<bool> UserHasBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(ub => ub.UserId == userId && ub.BranchId == branchId && ub.IsActive, cancellationToken);
        }

        /// <summary>
        /// Busca relaciones usuario-sucursal con criterios avanzados
        /// </summary>
        /// <param name="userId">ID del usuario (opcional)</param>
        /// <param name="branchId">ID de la sucursal (opcional)</param>
        /// <param name="isPrimary">Indica si es la sucursal principal (opcional)</param>
        /// <param name="isActive">Indica si está activa (opcional)</param>
        /// <param name="includeUser">Indica si se debe incluir el usuario</param>
        /// <param name="includeBranch">Indica si se debe incluir la sucursal</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de relaciones usuario-sucursal que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<UserBranch> Items, int TotalCount)> SearchAsync(
            Guid? userId = null,
            Guid? branchId = null,
            bool? isPrimary = null,
            bool? isActive = null,
            bool includeUser = false,
            bool includeBranch = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<UserBranch> query = _dbSet;

            // Incluir relaciones según se solicite
            if (includeUser)
            {
                query = query.Include(ub => ub.User);
            }

            if (includeBranch)
            {
                query = query.Include(ub => ub.Branch)
                    .ThenInclude(b => b.Organization);
            }

            // Aplicar filtros
            if (userId.HasValue)
            {
                query = query.Where(ub => ub.UserId == userId.Value);
            }

            if (branchId.HasValue)
            {
                query = query.Where(ub => ub.BranchId == branchId.Value);
            }

            if (isPrimary.HasValue)
            {
                query = query.Where(ub => ub.IsPrimary == isPrimary.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(ub => ub.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(ub => ub.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(ub => ub.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Quita la marca de sucursal principal de todas las sucursales de un usuario excepto la especificada
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="exceptBranchId">ID de la sucursal a excluir</param>
        /// <param name="modifiedBy">ID del usuario que realiza el cambio</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task</returns>
        private async Task RemovePrimaryFlagFromOtherBranchesAsync(Guid userId, Guid exceptBranchId, Guid modifiedBy, CancellationToken cancellationToken = default)
        {
            var primaryBranches = await _dbSet
                .Where(ub => ub.UserId == userId && ub.BranchId != exceptBranchId && ub.IsPrimary && ub.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var branch in primaryBranches)
            {
                branch.IsPrimary = false;
                branch.LastModifiedBy = modifiedBy.ToString();
                branch.LastModifiedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Obtiene una relación usuario-sucursal por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID de la relación usuario-sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal encontrada o null</returns>
        public override async Task<UserBranch> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.User)
                .Include(ub => ub.Branch)
                    .ThenInclude(b => b.Organization)
                .FirstOrDefaultAsync(ub => ub.Id == id && ub.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las relaciones usuario-sucursal activas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal activas</returns>
        public override async Task<IReadOnlyList<UserBranch>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ub => ub.User)
                .Include(ub => ub.Branch)
                .Where(ub => ub.IsActive)
                .OrderBy(ub => ub.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
