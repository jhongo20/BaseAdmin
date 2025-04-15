using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de permisos
    /// </summary>
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        /// <param name="name">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos por módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del módulo</returns>
        Task<IReadOnlyList<Permission>> GetByModuleAsync(Guid moduleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos asignados a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        Task<IReadOnlyList<Permission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos asignados a un usuario a través de sus roles
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos asignados al usuario</returns>
        Task<IReadOnlyList<Permission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico en un módulo
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso en el módulo</returns>
        Task<bool> UserHasPermissionInModuleAsync(Guid userId, Guid moduleId, string permissionName, CancellationToken cancellationToken = default);
    }
}
