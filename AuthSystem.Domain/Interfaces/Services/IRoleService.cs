using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de roles
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        /// <param name="role">Entidad de rol</param>
        /// <param name="permissionIds">Lista opcional de IDs de permisos a asignar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol creado</returns>
        Task<Role> CreateRoleAsync(
            Role role, 
            IEnumerable<Guid> permissionIds = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        /// <param name="role">Entidad de rol con cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol actualizado</returns>
        Task<Role> UpdateRoleAsync(
            Role role, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el rol fue eliminado correctamente</returns>
        Task<bool> DeleteRoleAsync(
            Guid roleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        Task<Role> GetRoleByIdAsync(
            Guid roleId, 
            bool includePermissions = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        Task<Role> GetRoleByNameAsync(
            string name, 
            Guid? organizationId = null, 
            bool includePermissions = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los roles
        /// </summary>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles</returns>
        Task<IReadOnlyList<Role>> GetAllRolesAsync(
            bool includePermissions = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene roles por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles de la organización</returns>
        Task<IReadOnlyList<Role>> GetRolesByOrganizationAsync(
            Guid organizationId, 
            bool includePermissions = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene roles por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles del usuario</returns>
        Task<IReadOnlyList<Role>> GetRolesByUserAsync(
            Guid userId, 
            bool includePermissions = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna permisos a un rol
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
        /// Elimina permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">Lista de IDs de permisos a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de relaciones eliminadas</returns>
        Task<int> RemovePermissionsFromRoleAsync(
            Guid roleId, 
            IEnumerable<Guid> permissionIds, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del rol</returns>
        Task<IReadOnlyList<Permission>> GetPermissionsByRoleAsync(
            Guid roleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un rol tiene un permiso específico
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el rol tiene el permiso</returns>
        Task<bool> HasPermissionAsync(
            Guid roleId, 
            Guid permissionId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca roles con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="isSystemRole">Indica si es un rol de sistema (opcional)</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de roles que cumplen los criterios</returns>
        Task<(IReadOnlyList<Role> Items, int TotalCount)> SearchRolesAsync(
            string searchTerm = null,
            Guid? organizationId = null,
            bool? isSystemRole = null,
            bool includePermissions = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Clona un rol existente
        /// </summary>
        /// <param name="sourceRoleId">ID del rol fuente</param>
        /// <param name="newRoleName">Nombre del nuevo rol</param>
        /// <param name="newRoleDescription">Descripción del nuevo rol</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="clonePermissions">Indica si se deben clonar los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Nuevo rol creado</returns>
        Task<Role> CloneRoleAsync(
            Guid sourceRoleId,
            string newRoleName,
            string newRoleDescription,
            Guid? organizationId = null,
            bool clonePermissions = true,
            CancellationToken cancellationToken = default);
    }
}
