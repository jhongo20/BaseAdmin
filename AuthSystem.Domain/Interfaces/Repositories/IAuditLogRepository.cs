using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de logs de auditoría
    /// </summary>
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        /// <summary>
        /// Obtiene logs de auditoría por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría del usuario</returns>
        Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por tipo de acción
        /// </summary>
        /// <param name="actionType">Tipo de acción</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría del tipo especificado</returns>
        Task<IReadOnlyList<AuditLog>> GetByActionTypeAsync(AuditActionType actionType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por entidad
        /// </summary>
        /// <param name="entityName">Nombre de la entidad</param>
        /// <param name="entityId">ID de la entidad (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para la entidad</returns>
        Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para la organización</returns>
        Task<IReadOnlyList<AuditLog>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para la sucursal</returns>
        Task<IReadOnlyList<AuditLog>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por rango de fechas
        /// </summary>
        /// <param name="startDate">Fecha de inicio</param>
        /// <param name="endDate">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría en el rango de fechas</returns>
        Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por módulo
        /// </summary>
        /// <param name="moduleName">Nombre del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría para el módulo</returns>
        Task<IReadOnlyList<AuditLog>> GetByModuleAsync(string moduleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría por nivel de severidad
        /// </summary>
        /// <param name="severity">Nivel de severidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría con el nivel de severidad especificado</returns>
        Task<IReadOnlyList<AuditLog>> GetBySeverityAsync(string severity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene logs de auditoría de acciones fallidas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de logs de auditoría de acciones fallidas</returns>
        Task<IReadOnlyList<AuditLog>> GetFailedActionsAsync(CancellationToken cancellationToken = default);

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
        Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAsync(
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
            CancellationToken cancellationToken = default);
    }
}
