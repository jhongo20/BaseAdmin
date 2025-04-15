using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de relaciones entre roles y permisos
    /// </summary>
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        /// <summary>
        /// Obtiene relaciones de rol-permiso por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso</returns>
        Task<IReadOnlyList<RolePermission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene relaciones de rol-permiso por permiso
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso</returns>
        Task<IReadOnlyList<RolePermission>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una relación rol-permiso específica
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación rol-permiso encontrada o null</returns>
        Task<RolePermission> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene relaciones de rol-permiso activas por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso activas</returns>
        Task<IReadOnlyList<RolePermission>> GetActiveByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna múltiples permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">Lista de IDs de permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso creadas</returns>
        Task<IReadOnlyList<RolePermission>> AssignPermissionsToRoleAsync(
            Guid roleId, 
            IEnumerable<Guid> permissionIds, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina todos los permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los permisos de un usuario a través de sus roles
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones rol-permiso para el usuario</returns>
        Task<IReadOnlyList<RolePermission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
