using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de relaciones entre usuarios y roles
    /// </summary>
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        /// <summary>
        /// Obtiene relaciones de usuario-rol por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol</returns>
        Task<IReadOnlyList<UserRole>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene relaciones de usuario-rol por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol</returns>
        Task<IReadOnlyList<UserRole>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una relación usuario-rol específica
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-rol encontrada o null</returns>
        Task<UserRole> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene relaciones de usuario-rol activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol activas</returns>
        Task<IReadOnlyList<UserRole>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna múltiples roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">Lista de IDs de roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol creadas</returns>
        Task<IReadOnlyList<UserRole>> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina todas las asignaciones de roles para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RemoveAllRolesFromUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
