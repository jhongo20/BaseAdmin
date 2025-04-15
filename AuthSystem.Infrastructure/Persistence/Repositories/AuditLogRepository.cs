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
    /// Implementación del repositorio de logs de auditoría
    /// </summary>
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene logs de auditoría por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría del usuario</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.UserId == userId && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por tipo de acción
        /// </summary>
        /// <param name="actionType">Tipo de acción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría del tipo especificado</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByActionTypeAsync(AuditActionType actionType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.ActionType == actionType && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por entidad
        /// </summary>
        /// <param name="entityName">Nombre de la entidad</param>
        /// <param name="entityId">ID de la entidad (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para la entidad</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentException("El nombre de la entidad no puede ser nulo o vacío", nameof(entityName));
            }

            var query = _dbSet
                .Where(al => al.EntityName == entityName && al.IsActive);

            if (!string.IsNullOrEmpty(entityId))
            {
                query = query.Where(al => al.EntityId == entityId);
            }

            return await query
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para la organización</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.OrganizationId == organizationId && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para la sucursal</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.BranchId == branchId && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por rango de fechas
        /// </summary>
        /// <param name="startDate">Fecha de inicio</param>
        /// <param name="endDate">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría en el rango de fechas</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.CreatedAt >= startDate && al.CreatedAt <= endDate && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por módulo
        /// </summary>
        /// <param name="moduleName">Nombre del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para el módulo</returns>
        public async Task<IReadOnlyList<AuditLog>> GetByModuleAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentException("El nombre del módulo no puede ser nulo o vacío", nameof(moduleName));
            }

            return await _dbSet
                .Where(al => al.ModuleName == moduleName && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría por nivel de severidad
        /// </summary>
        /// <param name="severity">Nivel de severidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría con el nivel de severidad especificado</returns>
        public async Task<IReadOnlyList<AuditLog>> GetBySeverityAsync(string severity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(severity))
            {
                throw new ArgumentException("El nivel de severidad no puede ser nulo o vacío", nameof(severity));
            }

            return await _dbSet
                .Where(al => al.Severity == severity && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría de acciones fallidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría de acciones fallidas</returns>
        public async Task<IReadOnlyList<AuditLog>> GetFailedActionsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => !al.IsSuccessful && al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene logs de auditoría con búsqueda avanzada
        /// </summary>
        /// <param name="userId">ID del usuario (opcional)</param>
        /// <param name="actionType">Tipo de acción (opcional)</param>
        /// <param name="entityName">Nombre de la entidad (opcional)</param>
        /// <param name="startDate">Fecha de inicio (opcional)</param>
        /// <param name="endDate">Fecha de fin (opcional)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="branchId">ID de la sucursal (opcional)</param>
        /// <param name="moduleName">Nombre del módulo (opcional)</param>
        /// <param name="severity">Nivel de severidad (opcional)</param>
        /// <param name="isSuccessful">Indica si la acción fue exitosa (opcional)</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de logs de auditoría que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAsync(
            Guid? userId = null,
            AuditActionType? actionType = null,
            string entityName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Guid? organizationId = null,
            Guid? branchId = null,
            string moduleName = null,
            string severity = null,
            bool? isSuccessful = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AuditLog> query = _dbSet
                .Where(al => al.IsActive);

            // Aplicar filtros
            if (userId.HasValue)
            {
                query = query.Where(al => al.UserId == userId.Value);
            }

            if (actionType.HasValue)
            {
                query = query.Where(al => al.ActionType == actionType.Value);
            }

            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(al => al.EntityName == entityName);
            }

            if (startDate.HasValue)
            {
                query = query.Where(al => al.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(al => al.CreatedAt <= endDate.Value);
            }

            if (organizationId.HasValue)
            {
                query = query.Where(al => al.OrganizationId == organizationId.Value);
            }

            if (branchId.HasValue)
            {
                query = query.Where(al => al.BranchId == branchId.Value);
            }

            if (!string.IsNullOrEmpty(moduleName))
            {
                query = query.Where(al => al.ModuleName == moduleName);
            }

            if (!string.IsNullOrEmpty(severity))
            {
                query = query.Where(al => al.Severity == severity);
            }

            if (isSuccessful.HasValue)
            {
                query = query.Where(al => al.IsSuccessful == isSuccessful.Value);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderByDescending(al => al.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene un log de auditoría por ID
        /// </summary>
        /// <param name="id">ID del log de auditoría</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Log de auditoría encontrado o null</returns>
        public override async Task<AuditLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(al => al.Id == id && al.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los logs de auditoría activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría activos</returns>
        public override async Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.IsActive)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
