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
    /// Implementación del repositorio de sucursales
    /// </summary>
    public class BranchRepository : Repository<Branch>, IBranchRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene una sucursal por su código
        /// </summary>
        /// <param name="code">Código de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal encontrada o null</returns>
        public async Task<Branch> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("El código de la sucursal no puede ser nulo o vacío", nameof(code));
            }

            return await _dbSet
                .Include(b => b.Organization)
                .Include(b => b.UserBranches)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.Code == code && b.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene una sucursal por su código y organización
        /// </summary>
        /// <param name="code">Código de la sucursal</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal encontrada o null</returns>
        public async Task<Branch> GetByCodeAndOrganizationAsync(string code, Guid organizationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("El código de la sucursal no puede ser nulo o vacío", nameof(code));
            }

            return await _dbSet
                .Include(b => b.Organization)
                .Include(b => b.UserBranches)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.Code == code && b.OrganizationId == organizationId && b.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene la sede principal de una organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal principal o null</returns>
        public async Task<Branch> GetHeadquartersAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Organization)
                .Include(b => b.UserBranches)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.OrganizationId == organizationId && b.IsHeadquarters && b.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene la sucursal principal de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal principal del usuario o null</returns>
        public async Task<Branch> GetPrimaryBranchForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userBranch = await _context.UserBranches
                .Include(ub => ub.Branch)
                    .ThenInclude(b => b.Organization)
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.IsPrimary && ub.IsActive && ub.Branch.IsActive, cancellationToken);

            return userBranch?.Branch;
        }

        /// <summary>
        /// Obtiene sucursales por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales de la organización</returns>
        public async Task<IReadOnlyList<Branch>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Organization)
                .Where(b => b.OrganizationId == organizationId && b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene sucursales por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales del usuario</returns>
        public async Task<IReadOnlyList<Branch>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Organization)
                .Include(b => b.UserBranches)
                    .ThenInclude(ub => ub.User)
                .Where(b => b.UserBranches.Any(ub => ub.UserId == userId && ub.IsActive) && b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca sucursales con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, código)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="isActive">Indica si está activa (opcional)</param>
        /// <param name="includeUsers">Indica si se deben incluir los usuarios</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de sucursales que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<Branch> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            Guid? organizationId = null,
            bool? isActive = null,
            bool includeUsers = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Branch> query = _dbSet
                .Include(b => b.Organization);

            // Incluir relaciones según se solicite
            if (includeUsers)
            {
                query = query.Include(b => b.UserBranches)
                    .ThenInclude(ub => ub.User);
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(searchTerm) ||
                    b.Code.ToLower().Contains(searchTerm) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }

            if (organizationId.HasValue)
            {
                query = query.Where(b => b.OrganizationId == organizationId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(b => b.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(b => b.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene una sucursal por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal encontrada o null</returns>
        public override async Task<Branch> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Organization)
                .Include(b => b.UserBranches)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las sucursales activas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales activas</returns>
        public override async Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Organization)
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
