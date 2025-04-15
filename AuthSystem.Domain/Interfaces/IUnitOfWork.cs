using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces.Repositories;

namespace AuthSystem.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para la unidad de trabajo que coordina múltiples operaciones en una transacción
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Repositorio de usuarios
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Repositorio de roles
        /// </summary>
        IRoleRepository Roles { get; }

        /// <summary>
        /// Repositorio de permisos
        /// </summary>
        IPermissionRepository Permissions { get; }

        /// <summary>
        /// Repositorio de módulos
        /// </summary>
        IModuleRepository Modules { get; }

        /// <summary>
        /// Repositorio de organizaciones
        /// </summary>
        IOrganizationRepository Organizations { get; }

        /// <summary>
        /// Repositorio de sucursales
        /// </summary>
        IBranchRepository Branches { get; }

        /// <summary>
        /// Repositorio de relaciones usuario-rol
        /// </summary>
        IUserRoleRepository UserRoles { get; }

        /// <summary>
        /// Repositorio de relaciones usuario-sucursal
        /// </summary>
        IUserBranchRepository UserBranches { get; }

        /// <summary>
        /// Repositorio de relaciones rol-permiso
        /// </summary>
        IRolePermissionRepository RolePermissions { get; }

        /// <summary>
        /// Repositorio de sesiones de usuario
        /// </summary>
        IUserSessionRepository UserSessions { get; }

        /// <summary>
        /// Repositorio de logs de auditoría
        /// </summary>
        IAuditLogRepository AuditLogs { get; }

        /// <summary>
        /// Guarda todos los cambios realizados en el contexto
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades modificadas</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicia una nueva transacción
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte la transacción actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
